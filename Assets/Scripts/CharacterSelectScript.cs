using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectScript : MonoBehaviour
{
    public GameObject[] characters;
    int characterIndex;

    public GameObject inputfield;
    string characterName;

    [Range(1, 4)]
    public int playerCount = 2;
    public List<int> selectedCharacterIndices = new List<int>();
    public List<string> selectedPlayerNames = new List<string>();

    public SceneChanger sceneChanger;

    [Header("UI")]
    public TMP_Text playerCountText;
    public Button playButton;                     // <-- reference to PlayButton

    [Header("Notifications")]
    public GameObject notificationPanel;
    public TMP_Text notificationText;

    private void Awake()
    {
        characterIndex = 0;
        foreach (GameObject character in characters)
        {
            character.SetActive(false);
        }

        if (characters.Length > 0)
        {
            characters[characterIndex].SetActive(true);
        }

        selectedCharacterIndices.Clear();
        selectedPlayerNames.Clear();

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }

        // Play should be disabled until all players are added
        UpdatePlayButtonInteractable();
    }

    private void Start()
    {
        UpdatePlayerCountLabel();
    }

    public void NextCharacter()
    {
        if (characters.Length == 0) return;

        characters[characterIndex].SetActive(false);
        characterIndex++;

        if (characterIndex == characters.Length)
        {
            characterIndex = 0;
        }
        characters[characterIndex].SetActive(true);
    }

    public void PreviousCharacter()
    {
        if (characters.Length == 0) return;

        characters[characterIndex].SetActive(false);
        characterIndex--;

        if (characterIndex == -1)
        {
            characterIndex = characters.Length - 1;
        }
        characters[characterIndex].SetActive(true);
    }

    // Called by "Add Player" button
    public void AddCurrentCharacterForNextPlayer()
    {
        if (selectedCharacterIndices.Count >= 4)
        {
            ShowNotification("Maximum of 4 players reached.");
            return;
        }

        characterName = inputfield.GetComponent<TMP_InputField>().text.Trim();

        if (characterName.Length <= 3)
        {
            ShowNotification("Name must be at least 4 characters.");
            inputfield.GetComponent<TMP_InputField>().Select();
            return;
        }

        // Check if this name was already added
        if (selectedPlayerNames.Contains(characterName))
        {
            int existingIndex = selectedPlayerNames.IndexOf(characterName);
            ShowNotification($"Player {existingIndex + 1} has already been added.");
            return;
        }

        // Add new player
        selectedCharacterIndices.Add(characterIndex);
        selectedPlayerNames.Add(characterName);

        playerCount = Mathf.Clamp(selectedCharacterIndices.Count, 1, 4);
        UpdatePlayerCountLabel();

        int playerNumber = selectedPlayerNames.Count;
        ShowNotification($"Player {playerNumber} added.");

        // Enable Play only when we already added as many players
        // as the chosen playerCount slider/value
        UpdatePlayButtonInteractable();
    }

    public void IncreasePlayerCount()
    {
        playerCount = Mathf.Clamp(playerCount + 1, 1, 4);
        UpdatePlayerCountLabel();
        UpdatePlayButtonInteractable();
    }

    public void DecreasePlayerCount()
    {
        playerCount = Mathf.Clamp(playerCount - 1, 1, 4);
        UpdatePlayerCountLabel();
        UpdatePlayButtonInteractable();
    }

    private void UpdatePlayerCountLabel()
    {
        if (playerCountText != null)
        {
            playerCountText.text = playerCount.ToString();
        }
    }

    private void UpdatePlayButtonInteractable()
    {
        // Play becomes active only when we have created exactly the
        // number of players requested in playerCount
        if (playButton != null)
        {
            bool ready = selectedCharacterIndices.Count == playerCount &&
                         selectedCharacterIndices.Count > 0;
            playButton.interactable = ready;
        }
    }

    // Simple notification helper
    public void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
        }

        if (notificationPanel != null)
        {
            notificationPanel.SetActive(true);
        }
    }

    // Hook this to the OK button on the notification panel
    public void HideNotification()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    public void Play()
    {
        if (selectedCharacterIndices.Count == 0)
        {
            ShowNotification("Add at least one player before starting.");
            return;
        }

        // Save first player's name as base name
        string baseName = selectedPlayerNames[0];
        PlayerPrefs.SetString("PlayerName", baseName);

        playerCount = Mathf.Clamp(selectedCharacterIndices.Count, 1, 4);
        PlayerPrefs.SetInt("PlayerCount", playerCount);

        for (int i = 0; i < selectedCharacterIndices.Count; i++)
        {
            PlayerPrefs.SetInt($"SelectedCharacter_{i}", selectedCharacterIndices[i]);
            PlayerPrefs.SetString($"PlayerName_{i}", selectedPlayerNames[i]);
        }

        PlayerPrefs.SetInt("SelectedCharacter", selectedCharacterIndices[0]);

        StartCoroutine(sceneChanger.Delay("play", selectedCharacterIndices[0], baseName));
    }
}