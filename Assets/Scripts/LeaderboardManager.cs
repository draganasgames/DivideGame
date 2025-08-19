using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class LeaderboardEntryData
{
    public string playerName;
    public int score;

    public LeaderboardEntryData(string name, int score)
    {
        playerName = name;
        this.score = score;
    }
}

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private const string PlayerNameKey = "PlayerName";
    private const string LeaderboardKey = "Leaderboard";

    public string PlayerName { get; private set; }
    public List<LeaderboardEntryData> Entries { get; private set; } = new List<LeaderboardEntryData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadPlayerName();
        LoadLeaderboard();
        EnsurePlayerEntry();
    }

    // ----- Player Name -----
    public bool HasPlayerName => !string.IsNullOrEmpty(PlayerName);

    public void SetPlayerName(string name)
    {
        PlayerName = name;
        PlayerPrefs.SetString(PlayerNameKey, name);
        PlayerPrefs.Save();
        EnsurePlayerEntry();
    }

    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(PlayerNameKey, "Unknown");
    }

    private void LoadPlayerName()
    {
        PlayerName = PlayerPrefs.GetString(PlayerNameKey, "");
    }

    // ----- Leaderboard -----
    private void EnsurePlayerEntry()
    {
        if (string.IsNullOrEmpty(PlayerName)) return;

        var entry = Entries.Find(e => e.playerName == PlayerName);
        int savedBest = PlayerPrefs.GetInt("BestScore", 0);

        if (entry == null)
        {
            Entries.Add(new LeaderboardEntryData(PlayerName, savedBest));
            SaveLeaderboard();
        }
        else if (savedBest > entry.score)
        {
            entry.score = savedBest;
            SaveLeaderboard();
        }
    }

    public void AddPlayerIfNotExists(string playerName)
    {
        var entry = Entries.Find(e => e.playerName == playerName);
        if (entry == null)
        {
            Entries.Add(new LeaderboardEntryData(playerName, 0));
            SaveLeaderboard();
        }
    }

    public void UpdatePlayerScore(string playerName, int score)
    {
        var entry = Entries.Find(e => e.playerName == playerName);
        if (entry != null)
        {
            // If the new score is higher, update
            if (score > entry.score)
                entry.score = score;
        }
        else
        {
            // If the player is not there, add them
            Entries.Add(new LeaderboardEntryData(playerName, score));
        }

        // Sort top 20
        Entries = Entries.OrderByDescending(e => e.score).Take(20).ToList();
        SaveLeaderboard();
    }

    public void UpdatePlayerScoreIfHigher(string playerName, int newScore)
    {
        if (string.IsNullOrEmpty(playerName)) return;

        var entry = Entries.Find(e => e.playerName == playerName);
        if (entry == null)
        {
            Entries.Add(new LeaderboardEntryData(playerName, newScore));
        }
        else if (newScore > entry.score)
        {
            entry.score = newScore;
        }

        Entries = Entries.OrderByDescending(e => e.score).Take(20).ToList();
        SaveLeaderboard();
    }

    public void AddScore(string playerName, int score)
    {
        var entry = Entries.Find(e => e.playerName == playerName);
        if (entry != null)
        {
            entry.score += score;
        }
        else
        {
            Entries.Add(new LeaderboardEntryData(playerName, score));
        }

        Entries = Entries.OrderByDescending(e => e.score).Take(20).ToList();
        SaveLeaderboard();
    }

    public List<LeaderboardEntryData> GetEntries()
    {
        return Entries;
    }

    private void SaveLeaderboard()
    {
        string json = JsonUtility.ToJson(new Wrapper { list = Entries });
        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();
    }

    private void LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LeaderboardKey, "");
        if (!string.IsNullOrEmpty(json))
        {
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(json);
            if (wrapper != null && wrapper.list != null)
                Entries = wrapper.list;
        }
        else
        {
            Entries = new List<LeaderboardEntryData>();
        }
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<LeaderboardEntryData> list;
    }
}