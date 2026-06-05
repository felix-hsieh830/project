using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("結算 UI (Game Over)")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalDistanceText;
    public TextMeshProUGUI finalKillText;

    [Header("Boss 獎勵 UI (Rewards)")]
    public GameObject rewardPanel;
    public TextMeshProUGUI btnAText;
    public TextMeshProUGUI btnBText;
    public TextMeshProUGUI btnCText;
    public GameObject buttonCObject;

    [Header("暫停 UI (Pause)")]
    public GameObject pausePanel;
    private bool isPaused = false;

    [Header("Boss 生成設定 (以距離為準)")]
    public float nextBossDistance = 450f;
    public float bossInterval = 450f;
    private int bossSpawnCount = 0;
    public float bossOffset = 10f;

    [Header("Boss 實體設定")]
    // 🌟 這裡改成陣列 (Array)，讓你可以放進去 3 隻（甚至以後更多隻）小 Boss！
    public GameObject[] smallBossPrefabs;
    public GameObject bigBossPrefab;
    public float bossSpawnDistance = 55f;

    private bool isBigBossReward = false;
    private PlayerStats playerStats;

    void Start()
    {
        playerStats = FindAnyObjectByType<PlayerStats>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameOverPanel.activeSelf && !rewardPanel.activeSelf)
            {
                if (isPaused) ResumeGameFromPause();
                else PauseGame();
            }
        }

        if (playerStats != null)
        {
            float playerDistance = playerStats.transform.position.z - 30f;

            if (playerDistance >= nextBossDistance - bossSpawnDistance)
            {
                bossSpawnCount++;
                bool isBigBoss = (bossSpawnCount % 4 == 0);
                float bossWorldZ = (nextBossDistance + bossOffset) + 35f;

                Debug.Log($"🚩 進入 Boss 視線範圍！Boss 已在世界座標 {bossWorldZ} m 處提早降落等待！");

                SpawnBoss(isBigBoss, bossWorldZ);
                nextBossDistance += bossInterval;
            }
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGameFromPause()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        Debug.Log("打開設定介面！");
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Debug.Log("退出遊戲！");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void ShowGameOver(int distance, int kills)
    {
        gameOverPanel.SetActive(true);
        finalDistanceText.text = "最終距離: " + FormatNumber(distance) + " m";
        finalKillText.text = "總擊殺數: " + FormatNumber(kills);
        int bestDistance = PlayerPrefs.GetInt("BestDistance", 0);
        if (distance > bestDistance) PlayerPrefs.SetInt("BestDistance", distance);
        int bestKills = PlayerPrefs.GetInt("BestKills", 0);
        if (kills > bestKills) PlayerPrefs.SetInt("BestKills", kills);
        PlayerPrefs.Save();

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowReward(bool isBigBoss)
    {
        isBigBossReward = isBigBoss;
        rewardPanel.SetActive(true);
        Time.timeScale = 0f;

        if (!isBigBossReward)
        {
            buttonCObject.SetActive(true);
            btnAText.text = "輕弩流派\n射速加快 (+50%)\n傷害降低 (-20%)";
            btnBText.text = "重砲流派\n攻擊加痛 (+15)\n攻速變慢 (-20%)";
            btnCText.text = "多重箭流派\n箭矢數量 +2\n傷害降低 (-50%)";
        }
        else
        {
            buttonCObject.SetActive(false);
            btnAText.text = "王牌力量\n攻擊力 x 2 倍 !!";
            btnBText.text = "狂暴極速\n攻擊速度 x 2 倍 !!";
        }
    }

    public void ChooseRewardA() { if (playerStats != null) { if (!isBigBossReward) { playerStats.attackSpeed += 1.5f; playerStats.baseDamage *= 0.8f; } else { playerStats.baseDamage *= 2f; } } ResumeGame(); }
    public void ChooseRewardB() { if (playerStats != null) { if (!isBigBossReward) { playerStats.baseDamage += 15f; playerStats.attackSpeed *= 0.8f; } else { playerStats.attackSpeed *= 2f; } } ResumeGame(); }
    public void ChooseRewardC() { if (playerStats != null) { if (!isBigBossReward) { playerStats.arrowCount += 2; playerStats.baseDamage *= 0.5f; } } ResumeGame(); }

    private void ResumeGame()
    {
        rewardPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.#") + "B";
        else if (number >= 1000000) return (number / 1000000f).ToString("0.#") + "M";
        else if (number >= 1000) return (number / 1000f).ToString("0.#") + "K";
        return Mathf.FloorToInt(number).ToString();
    }

    private void SpawnBoss(bool isBigBoss, float targetZ)
    {
        Transform playerTrans = playerStats.transform;
        Vector3 spawnPos = new Vector3(0, 1.5f, targetZ);
        float baseEnemyMaxHp = 50f;
        float scalingDistance = spawnPos.z - 30f;
        if (scalingDistance < 0) scalingDistance = 0;
        float stage = Mathf.Floor(scalingDistance / 40f);
        float scaleFactor = Mathf.Pow(1.1f, stage);
        float currentEnemyHpAtThisPosition = Mathf.Round(baseEnemyMaxHp * scaleFactor);
        GameObject spawnedBoss = null;

        if (isBigBoss)
        {
            Debug.Log("🔥 警告！大 Boss 即將降臨！");
            spawnedBoss = Instantiate(bigBossPrefab, spawnPos, Quaternion.identity);
            BossHealth bossScript = spawnedBoss.GetComponent<BossHealth>();
            if (bossScript != null) bossScript.SetupHealth(currentEnemyHpAtThisPosition * 5f);
        }
        else
        {
            // 防呆：確保陣列裡有放東西，才不會報錯
            if (smallBossPrefabs != null && smallBossPrefabs.Length > 0)
            {
                Debug.Log("⚠️ 準備戰鬥！小 Boss 出現！");

                // 🌟 核心邏輯：從 0 到「陣列長度」之間隨機抽一個數字
                int randomIndex = Random.Range(0, smallBossPrefabs.Length);
                GameObject selectedBossPrefab = smallBossPrefabs[randomIndex];

                spawnedBoss = Instantiate(selectedBossPrefab, spawnPos, Quaternion.identity);
                BossHealth bossScript = spawnedBoss.GetComponent<BossHealth>();
                if (bossScript != null) bossScript.SetupHealth(currentEnemyHpAtThisPosition * 2f);
            }
            else
            {
                Debug.LogError("❌ GameManager 裡面沒有放小 Boss 的 Prefab！請到 Inspector 設定！");
            }
        }
    }
}