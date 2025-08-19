using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicOnOff : MonoBehaviour
{
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private Button button;
    private Image image;
    private AudioSource audioSource;

    void Start()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        // Find music left over after DontDestroyOnLoad
        audioSource = FindObjectOfType<MusicManager>().GetComponent<AudioSource>();

        button.onClick.AddListener(ToggleSound);
        UpdateButtonIcon();
    }

    void ToggleSound()
    {
        MusicManager.Instance.ToggleSound();
        UpdateButtonIcon();
    }

    void UpdateButtonIcon()
    {
        image.sprite = MusicManager.Instance.soundOn ? soundOnSprite : soundOffSprite;
    }
}