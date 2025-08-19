using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{

    [Header("Refs")]
    public GridManager grid;
    public NumberGridManager numberGrid;
    public KeepSlot keepSlot;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI bestScoreText;
    public string gameOverSceneName = "GameOverScene";

    [Header("Rules")]
    public int minValue = 2;
    public int maxValue = 24;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip divisionSound;

    private int _bestScore;

    private const string BestScoreKey = "BestScore";

    public int Score { get; private set; }

    void Start()
    {
        _bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        UpdateScoreUI();
        ResetGame();
    }

    public void ResetGame()
    {
        Score = 0;
        UpdateScoreUI();

        grid.BuildGrid();               // create 3×3 slots (if not already created)
        numberGrid.FillStart(this);          // spawn the first 3
        keepSlot.Clear();               // clear the “keep”
    }

    private void UpdateScoreUI()
    {
        scoreText?.SetText(Score.ToString());

        // Update bestScore if the current Score is already higher
        if (Score > _bestScore)
        {
            _bestScore = Score;
            PlayerPrefs.SetInt(BestScoreKey, _bestScore);
            PlayerPrefs.Save();
        }

        // UI for best score
        if (bestScoreText != null)
            bestScoreText.SetText(_bestScore.ToString());

        // Update the leaderboard only if the current Score is higher than the previous score in the leaderboard
        if (!string.IsNullOrEmpty(LeaderboardManager.Instance.PlayerName))
        {
            LeaderboardManager.Instance.UpdatePlayerScore(LeaderboardManager.Instance.PlayerName, _bestScore);
        }
    }

    // Called from NumberTile when it is successfully placed on a TileSlot
    public void OnNumberPlaced(NumberTile placedTile, TileSlot slot)
    {
        // Closes interaction with this tile
        placedTile.LockInteraction();

        // Record slot occupancy
        slot.Place(placedTile);

        // Solve chain reactions of sharing starting from this slot
        ResolveDivisionChain(slot);

        // Fill the queue if the number is dequeued
        numberGrid.BackfillIfNeeded(this);

        // PCheck endgame: board full and no move from keep
        if (IsGameOver())
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }

    // Chain reactions: as long as some sharing around the center can - it works
    private void ResolveDivisionChain(TileSlot startingSlot)
    {
        if (startingSlot == null || startingSlot.Occupant == null) return;

        Queue<TileSlot> queue = new Queue<TileSlot>();
        queue.Enqueue(startingSlot);

        while (queue.Count > 0)
        {
            TileSlot current = queue.Dequeue();
            if (current == null || current.Occupant == null) continue;

            var currentTile = current.Occupant;
            int currentValue = currentTile.Value;

            List<TileSlot> neighbors = grid.GetNeighbors(current)
                .Where(n => n != null && n.Occupant != null).ToList();

            // --- 1. same numbers ---
            var sameValueTiles = neighbors
                .Where(n => n.Occupant.Value == currentValue)
                .Select(n => n.Occupant)
                .ToList();

            if (sameValueTiles.Count > 0)
            {
                // include the center itself
                sameValueTiles.Add(currentTile);

                // update score
                Score += currentValue * sameValueTiles.Count;
                UpdateScoreUI();

                if (audioSource != null && divisionSound != null)
                    audioSource.PlayOneShot(divisionSound);

                foreach (var tile in sameValueTiles.Distinct())
                {
                    DestroyTile(tile);
                }

                // add their neighbors to check
                foreach (var tile in sameValueTiles)
                {
                    foreach (var adj in grid.GetNeighbors(tile.CurrentSlot))
                    {
                        if (adj != null && adj.Occupant != null)
                            queue.Enqueue(adj);
                    }
                }

                continue; // skip further checking because they are all gone
            }

            // --- 2. divisibility ---
            List<(TileSlot slot, NumberTile tile, int newValue)> updates = new List<(TileSlot, NumberTile, int)>();
            List<NumberTile> toDestroy = new List<NumberTile>();

            foreach (var neighbor in neighbors)
            {
                var neighborTile = neighbor.Occupant;
                int a = currentTile.Value;
                int b = neighborTile.Value;

                // if current is a neighbor divisor
                if (b % a == 0 && b != a)
                {
                    updates.Add((neighbor, neighborTile, b / a));
                    toDestroy.Add(currentTile); // the divisor disappears
                }
                // if neighbor is a divisor of current
                else if (a % b == 0 && a != b)
                {
                    updates.Add((current, currentTile, a / b));
                    toDestroy.Add(neighborTile); // the divisor disappears
                }
            }

            if (updates.Count > 0)
            {
                // Divisor - smallest number between currentTile and neighborTile
                int divisor = updates.Min(u => u.newValue);

                // Number of split numbers - all participating tiles
                int dividedCount = updates.Count + 1; // +1 for the dealer

                foreach (var up in updates)
                {
                    up.tile.SetValue(up.newValue);

                    queue.Enqueue(up.slot); // check again the slot that changed

                    if (audioSource != null && divisionSound != null)
                        audioSource.PlayOneShot(divisionSound);
                }

                Score += divisor * dividedCount;
                UpdateScoreUI();

                foreach (var t in toDestroy.Distinct())
                {
                    DestroyTile(t);
                }

                continue;
            }
        }
    }

    private void DestroyTile(NumberTile tile)
    {
        if (tile == null) return;
        TileSlot slot = tile.CurrentSlot;
        if (slot != null && slot.Occupant == tile)
            slot.Clear();
        Destroy(tile.gameObject);
    }

    private bool IsGameOver()
    {
        if (!grid.IsFull())
        {
            return false;
        }

        var candidateValues = new List<int>();

        int queueRightmost = numberGrid.GetRightmostValueOrMinusOne();
        if (queueRightmost > 0) candidateValues.Add(queueRightmost);

        if (keepSlot.HasTile)
        {
            candidateValues.Add(keepSlot.Tile.Value);
        }

        if (!string.IsNullOrEmpty(LeaderboardManager.Instance.PlayerName))
        {
            LeaderboardManager.Instance.UpdatePlayerScoreIfHigher(
                LeaderboardManager.Instance.PlayerName,
                _bestScore
            );
        }

        if (!grid.IsFull())
        {
            foreach (var v in candidateValues)
            {
                if (grid.AnyDivisibleWith(v))
                {
                    return false;
                }
            }
        }

        return true;
    }
}