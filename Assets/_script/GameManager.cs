using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 🌟 重啟遊戲必須要有這行！

public class GameManager : MonoBehaviour
{
    [Header("結算 UI")]
    public GameObject gameOverPanel; 
    public TextMeshProUGUI finalDistanceText;
    public TextMeshProUGUI finalKillText;

    public void ShowGameOver(int distance, int kills)
    {
        // 1. 打開結算面板
        gameOverPanel.SetActive(true);

        // 2. 更新文字
        finalDistanceText.text = "最終距離: " + distance + " m";
        finalKillText.text = "總擊殺數: " + kills;

        // 3. 時間停止
        Time.timeScale = 0f; 
    }

    public void RestartGame()
    {
        // 1. 恢復時間流動
        Time.timeScale = 1f;
        // 2. 重新讀取當前關卡
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}