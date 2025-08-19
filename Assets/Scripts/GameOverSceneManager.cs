using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSceneManager : MonoBehaviour
{
    public string gameplaySceneName = "GameplayScene";
    public LeaderboardPanel leaderboardPanel;

    public void OnStartButtonClicked()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnExitButtonClicked()
    {
        Application.Quit();
    }

    public void OnLeaderboardButtonClicked()
    {
        leaderboardPanel.OpenLeaderboard();
    }
}
