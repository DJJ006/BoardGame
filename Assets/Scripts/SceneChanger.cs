using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public SaveLoadScript saveLoadScript;
    public OverlayScript overlayScript;

    public void closeGame()
    {
        StartCoroutine(Delay("quit", -1, ""));
    }

    public IEnumerator Delay(string command, int characterIndex, string characterName)
    {
        if (string.Equals(command, "quit", System.StringComparison.OrdinalIgnoreCase))
        {
            if (overlayScript != null)
            {
                yield return overlayScript.FadeOut(0.4f);
            }

            PlayerPrefs.DeleteAll();

#if UNITY_EDITOR
            // Stop play mode in the editor
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else if (string.Equals(command, "play", System.StringComparison.OrdinalIgnoreCase))
        {
            if (overlayScript != null)
            {
                yield return overlayScript.FadeOut(0.4f);
            }

            if (saveLoadScript != null)
            {
                saveLoadScript.SaveGame(characterIndex, characterName);
            }

            SceneManager.LoadScene(1, LoadSceneMode.Single);
        }
        else if (string.Equals(command, "menu", System.StringComparison.OrdinalIgnoreCase))
        {
            if (overlayScript != null)
            {
                yield return overlayScript.FadeOut(0.4f);
            }

            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
    }

    public void GoToMenu()
    {
        StartCoroutine(Delay("menu", -1, ""));
    }
}
