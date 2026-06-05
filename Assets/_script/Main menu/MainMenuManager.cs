using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI 面板綁定")]
    public GameObject settingsPanel;
    public TextMeshProUGUI bestRecordText; // 🌟 用來顯示最高紀錄的文字

    void Start()
    {
        // 🌟 遊戲一打開，就去記憶卡讀取紀錄
        int bestDist = PlayerPrefs.GetInt("BestDistance", 0);
        int bestKill = PlayerPrefs.GetInt("BestKills", 0);

        if (bestRecordText != null)
        {
            bestRecordText.text = $"最高紀錄: {FormatNumber(bestDist)}m  |  擊殺: {FormatNumber(bestKill)}";
        }
    }

    public void PlayGame() { SceneManager.LoadScene(1); }
    public void OpenSettings() { if (settingsPanel != null) settingsPanel.SetActive(true); }
    public void CloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.##") + "B";
        else if (number >= 1000000) return (number / 1000000f).ToString("0.##") + "M";
        else if (number >= 1000) return (number / 1000f).ToString("0.##") + "K";
        return Mathf.FloorToInt(number).ToString();
    }
}