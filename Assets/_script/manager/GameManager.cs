using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("結算 UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalDistanceText;
    public TextMeshProUGUI finalKillText;

    [Header("Boss 獎勵 UI")]
    public GameObject rewardPanel;
    public TextMeshProUGUI btnAText;
    public TextMeshProUGUI btnBText;
    public TextMeshProUGUI btnCText;
    public GameObject buttonCObject;

    [Header("暫停與生成")]
    public GameObject pausePanel;
    private bool isPaused = false;
    public float nextBossDistance = 450f;
    public float bossInterval = 450f;
    private int bossSpawnCount = 0;
    public float bossOffset = 10f;
    public GameObject[] smallBossPrefabs;
    public GameObject bigBossPrefab;
    public float bossSpawnDistance = 55f;

    private bool isBigBossReward = false;
    private PlayerStats playerStats;
    private bool isSpawning = false;

    [Header("Boss 獎勵箱")]
    public GameObject bossRewardChestPrefab;

    // 🌟 這裡改成 EnemyPlusOne
    public enum RewardType { Light, Heavy, Multi, BigAtk, BigSpd, Lifesteal, Resist, EnemyPlusOne, Magnet }
    private List<RewardType> currentOptions = new List<RewardType>();

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

        if (playerStats != null && !isSpawning)
        {
            float playerDistance = playerStats.transform.position.z - 30f;
            if (playerDistance >= nextBossDistance - bossSpawnDistance)
            {
                isSpawning = true;
                bossSpawnCount++;
                bool isBigBoss = (bossSpawnCount % 4 == 0);
                float bossWorldZ = (nextBossDistance + bossOffset) + 35f;
                SpawnBoss(isBigBoss, bossWorldZ);
                nextBossDistance += bossInterval;
                Invoke("ResetSpawnLock", 2.0f);
            }
        }
    }

    private void ResetSpawnLock() { isSpawning = false; }
    private void PauseGame() { isPaused = true; pausePanel.SetActive(true); Time.timeScale = 0f; }
    public void ResumeGameFromPause() { isPaused = false; pausePanel.SetActive(false); Time.timeScale = 1f; }
    public void ReturnToMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene(0); }
    public void QuitGame() { Application.Quit(); }

    public void ShowGameOver(int distance, int kills)
    {
        gameOverPanel.SetActive(true);
        finalDistanceText.text = "最終距離: " + FormatNumber(distance) + " m";
        finalKillText.text = "總擊殺數: " + FormatNumber(kills);
        int bestDist = PlayerPrefs.GetInt("BestDistance", 0);
        if (distance > bestDist) PlayerPrefs.SetInt("BestDistance", distance);
        int bestKill = PlayerPrefs.GetInt("BestKills", 0);
        if (kills > bestKill) PlayerPrefs.SetInt("BestKills", kills);
        PlayerPrefs.Save();
        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // 記得把時間恢復流動
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 重新讀取當前場景
    }

    public void ShowReward(bool isBigBoss)
    {
        EndEnemyPlusOneStage();
        isBigBossReward = isBigBoss;
        rewardPanel.SetActive(true);
        Time.timeScale = 0f;
        GenerateRewards();
    }

    public void RefreshRewards()
    {
        if (isBigBossReward) return;
        GenerateRewards();
    }

    private void GenerateRewards()
    {
        List<RewardType> pool = new List<RewardType>();

        if (!isBigBossReward)
        {
            pool.Add(RewardType.Light); pool.Add(RewardType.Heavy); pool.Add(RewardType.Multi);

            if (playerStats.lifestealLevel < 3) pool.Add(RewardType.Lifesteal);
            if (playerStats.collisionResistLevel < 3) pool.Add(RewardType.Resist);
            pool.Add(RewardType.EnemyPlusOne); // 🌟 只影響下一段 Boss 距離
            if (playerStats.magnetLevel < 3) pool.Add(RewardType.Magnet);
        }
        else
        {
            pool.Add(RewardType.BigAtk);
            pool.Add(RewardType.BigSpd);
        }

        for (int i = 0; i < pool.Count; i++)
        {
            RewardType temp = pool[i];
            int randomIndex = Random.Range(i, pool.Count);
            pool[i] = pool[randomIndex];
            pool[randomIndex] = temp;
        }

        currentOptions.Clear();
        buttonCObject.SetActive(!isBigBossReward);

        int optionCount = isBigBossReward ? 2 : 3;
        for (int i = 0; i < optionCount; i++)
        {
            if (i < pool.Count) currentOptions.Add(pool[i]);
        }

        if (currentOptions.Count > 0) UpdateButtonUI(btnAText, currentOptions[0]);
        if (currentOptions.Count > 1) UpdateButtonUI(btnBText, currentOptions[1]);
        if (currentOptions.Count > 2) UpdateButtonUI(btnCText, currentOptions[2]);
        else if (btnCText != null) btnCText.text = "";
    }

    private void UpdateButtonUI(TextMeshProUGUI btnText, RewardType type)
    {
        switch (type)
        {
            case RewardType.Light: btnText.text = "輕弩流派\n射速加快(+50%)\n傷害降低(-20%)"; break;
            case RewardType.Heavy: btnText.text = "重砲流派\n攻擊加痛(+15)\n攻速變慢(-20%)"; break;
            case RewardType.Multi: btnText.text = "多重箭流派\n箭矢數量 +2\n傷害降低(-50%)"; break;
            case RewardType.BigAtk: btnText.text = "王牌力量\n攻擊力 x 2倍 !!"; break;
            case RewardType.BigSpd: btnText.text = "狂暴極速\n攻擊速度 x 2倍 !!"; break;
            case RewardType.Lifesteal: btnText.text = $"吸血 Lv{playerStats.lifestealLevel + 1}\n傷害 {(playerStats.lifestealLevel + 1) * 10}% 轉為回血"; break;
            case RewardType.Resist: btnText.text = $"堅若磐石 Lv{playerStats.collisionResistLevel + 1}\n撞擊減傷 {(playerStats.collisionResistLevel + 1) * 10}%"; break;
            case RewardType.EnemyPlusOne: btnText.text = $"敵潮洶湧\n下一段怪物額外 +1!"; break; // 🌟 只影響下一段 Boss 距離
            case RewardType.Magnet: btnText.text = $"金幣磁鐵 Lv{playerStats.magnetLevel + 1}\n吸引半徑 {(playerStats.magnetLevel + 1) * 3}m"; break;
        }
    }

    public void ChooseRewardA() { ApplyReward(currentOptions[0]); ResumeGame(); }
    public void ChooseRewardB() { ApplyReward(currentOptions[1]); ResumeGame(); }
    public void ChooseRewardC()
    {
        if (currentOptions.Count <= 2) return;
        ApplyReward(currentOptions[2]);
        ResumeGame();
    }

    private void ApplyReward(RewardType type)
    {
        if (playerStats == null) return;

        switch (type)
        {
            case RewardType.Light: playerStats.attackSpeed += 1.5f; playerStats.baseDamage *= 0.8f; break;
            case RewardType.Heavy: playerStats.baseDamage += 15f; playerStats.attackSpeed *= 0.8f; break;
            case RewardType.Multi: playerStats.arrowCount += 2; playerStats.baseDamage *= 0.5f; break;
            case RewardType.BigAtk: playerStats.baseDamage *= 2f; break;
            case RewardType.BigSpd: playerStats.attackSpeed *= 2f; break;
            case RewardType.Lifesteal: playerStats.lifestealLevel++; break;
            case RewardType.Resist: playerStats.collisionResistLevel++; break;
            case RewardType.EnemyPlusOne:
                playerStats.extraEnemies = 1;
                Enemy.RefreshAllExtraEnemies(playerStats.extraEnemies);
                break; // 🌟 只影響下一段 Boss 距離
            case RewardType.Magnet: playerStats.magnetLevel++; break;
        }
    }

    private void ResumeGame() { rewardPanel.SetActive(false); Time.timeScale = 1f; }

    private void EndEnemyPlusOneStage()
    {
        if (playerStats == null || playerStats.extraEnemies <= 0) return;

        playerStats.extraEnemies = 0;
        Enemy.ClearAllExtraEnemies();
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
        Vector3 spawnPos = new Vector3(0, 1.5f, targetZ);
        float baseEnemyMaxHp = 50f;
        float scalingDistance = Mathf.Max(0, spawnPos.z - 30f);
        float stage = Mathf.Floor(scalingDistance / 40f);
        float currentEnemyHp = Mathf.Round(baseEnemyMaxHp * Mathf.Pow(1.1f, stage));
        GameObject spawnedBoss = null;

        if (isBigBoss)
        {
            spawnedBoss = Instantiate(bigBossPrefab, spawnPos, Quaternion.identity);
            if (spawnedBoss.GetComponent<BossHealth>() != null) spawnedBoss.GetComponent<BossHealth>().SetupHealth(currentEnemyHp * 5f);
        }
        else
        {
            if (smallBossPrefabs != null && smallBossPrefabs.Length > 0)
            {
                int randomIndex = Random.Range(0, smallBossPrefabs.Length);
                spawnedBoss = Instantiate(smallBossPrefabs[randomIndex], spawnPos, Quaternion.identity);
                if (spawnedBoss.GetComponent<BossHealth>() != null) spawnedBoss.GetComponent<BossHealth>().SetupHealth(currentEnemyHp * 2f);
            }
        }
    }
    public void SpawnBossRewardChest(Vector3 position, bool isBigBoss)
    {
        if (bossRewardChestPrefab == null) return;
        Vector3 spawnPos = new Vector3(position.x - 1f, -0.45f, position.z);
        GameObject chest = Instantiate(bossRewardChestPrefab, spawnPos, Quaternion.Euler(0, 180f, 0));

        // 把 isBigBoss 傳給箱子，讓它知道要跳哪種獎勵
        BossRewardChest chestScript = chest.GetComponent<BossRewardChest>();
        if (chestScript != null) chestScript.isBigBoss = isBigBoss;
    }
}
