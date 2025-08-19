using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsScreen;
    public LeaderboardPanel leaderboardPanel;

    public void OpenSettings()
    {
        settingsScreen.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsScreen.SetActive(false);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("GameplayScene");
    }

    public void GoHome()
    {
        SceneManager.LoadScene("StartGameScene");
    }

    public void OnLeaderboardButtonClicked()
    {
        leaderboardPanel.OpenLeaderboard();
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}