using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class StartScreenManager : MonoBehaviour
{
    public string gameplaySceneName = "GameplayScene";

    [Header("UI References")]
    public TMP_InputField nameInputField;
    public Button startButton;
    public LeaderboardPanel leaderboardPanel;

    private const string PLAYER_NAME_KEY = "PlayerNameText";
    private const int MAX_NAME_LENGTH = 30;

    void Start()
    {
        nameInputField.characterLimit = MAX_NAME_LENGTH;

        if (PlayerPrefs.HasKey(PLAYER_NAME_KEY))
        {
            // If we already have a name, hide the field, the button is active
            nameInputField.gameObject.SetActive(false);
            startButton.interactable = true;
        }
        else
        {
            // If no name, input field active, button disabled
            nameInputField.gameObject.SetActive(true);
            startButton.interactable = false;

            // Add listener only once
            nameInputField.onValueChanged.RemoveAllListeners();
            nameInputField.onValueChanged.AddListener(OnNameChanged);
        }
    }

    void OnNameChanged(string text)
    {
        if (text.Length > MAX_NAME_LENGTH)
            nameInputField.text = text.Substring(0, MAX_NAME_LENGTH);

        // Activate the button only if it is not empty
        startButton.interactable = !string.IsNullOrEmpty(nameInputField.text.Trim());
    }

    public void OnStartButtonClicked()
    {
        string playerName;

        if (PlayerPrefs.HasKey(PLAYER_NAME_KEY))
        {
            // If a name has already been entered before
            playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY);
        }
        else
        {
            playerName = nameInputField.text.Trim();
            PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
            PlayerPrefs.Save();
        }

        // Add a player to the leaderboard (if it doesn't already exist)
        LeaderboardManager.Instance.AddPlayerIfNotExists(playerName);

        SceneManager.LoadScene(gameplaySceneName);
    }

    public static string GetPlayerName()
    {
        return PlayerPrefs.GetString(PLAYER_NAME_KEY, "Unknown");
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