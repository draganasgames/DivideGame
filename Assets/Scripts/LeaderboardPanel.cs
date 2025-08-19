using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeaderboardPanel : MonoBehaviour
{
    public static LeaderboardPanel Instance;

    public Transform contentParent;
    public LeaderboardEntry entryPrefab;

    private bool isInitialized = false;

    private void OnEnable()
    {
        RefreshLeaderboard();
    }

    public void OpenLeaderboard()
    {
        gameObject.SetActive(true);


        if (!isInitialized)
        {
            RefreshLeaderboard();
            isInitialized = true;
        }
    }

    public void RefreshLeaderboard()
    {
        string playerName = LeaderboardManager.Instance.PlayerName;
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        if (!string.IsNullOrEmpty(playerName))
        {
            LeaderboardManager.Instance.UpdatePlayerScoreIfHigher(playerName, bestScore);
        }

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var e in LeaderboardManager.Instance.GetEntries())
        {
            var entry = Instantiate(entryPrefab, contentParent);
            entry.Init(e.playerName, bestScore);
        }
    }

    public void CloseLeaderboard()
    {
        gameObject.SetActive(false);
    }
}