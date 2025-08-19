using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardEntry : MonoBehaviour
{
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI scoreText;

    public void Init(string name, int score)
    {
        playerNameText.text = name;
        scoreText.text = score.ToString();
    }
}