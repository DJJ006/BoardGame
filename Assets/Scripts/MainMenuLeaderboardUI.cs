using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLeaderboardUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text leaderboardText;   // inside ScrollRect > Viewport > Content
    public ScrollRect scrollRect;      // optional: to reset scroll position

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (leaderboardText == null)
            return;

        LeaderboardData data = LeaderboardData.Load();

        if (data.entries.Count == 0)
        {
            leaderboardText.text = "No scores yet.";
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("LEADERBOARD");

            // show all entries, scrolling will handle overflow
            for (int i = 0; i < data.entries.Count; i++)
            {
                var e = data.entries[i];
                sb.AppendLine($"{i + 1}. {e.playerName} - {e.points} pts");
            }

            leaderboardText.text = sb.ToString();
        }

        // reset scroll to top when panel opens
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();   // ensure layout updated
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public void ClearLeaderboard()
    {
        PlayerPrefs.DeleteKey("Leaderboard");
        PlayerPrefs.Save();
        Refresh();
    }
}
