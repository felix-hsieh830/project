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
    public GameObject buttonCObject; // 控制第三個按鈕顯示/隱藏

    [Header("Boss 生成設定 (以距離為準)")]
    public float nextBossDistance = 450f; // 🌟 第一隻 Boss 固定出現的距離
    public float bossInterval = 450f;     // 🌟 之後每隔幾公尺出一隻
    private int bossSpawnCount = 0;        // 🌟 記住已經出了幾隻 Boss
    public float bossOffset = 10f;

    [Header("Boss 實體設定")]
    public GameObject smallBossPrefab;
    public GameObject bigBossPrefab;
    public float bossSpawnDistance = 55f; // 在玩家前方多遠的地方生成 Boss

    // 記住現在是大 Boss 還是小 Boss
    private bool isBigBossReward = false;

    // 用來偷偷修改玩家屬性的大腦
    private PlayerStats playerStats;

    void Start()
    {
        // 遊戲開始時，自動在場景中尋找主角的數值大腦
        playerStats = FindAnyObjectByType<PlayerStats>();
    }

    void Update()
    {
        if (playerStats != null)
        {
            float playerDistance = playerStats.transform.position.z - 30f;

            if (playerDistance >= nextBossDistance - bossSpawnDistance)
            {
                bossSpawnCount++;
                bool isBigBoss = (bossSpawnCount % 3 == 0);
                float bossWorldZ = (nextBossDistance+bossOffset) + 35f;

                Debug.Log($"🚩 進入 Boss 視線範圍！Boss 已在世界座標 {bossWorldZ} m 處提早降落等待！");

                // 把校正好的絕對座標傳給 Boss
                SpawnBoss(isBigBoss, bossWorldZ);

                // 更新下一隻 Boss 的距離
                nextBossDistance += bossInterval;
            }
        }
    }

    //  死亡與結算系統
    public void ShowGameOver(int distance, int kills)
    {
        gameOverPanel.SetActive(true);
        finalDistanceText.text = "最終距離: " + distance + " m";
        finalKillText.text = "總擊殺數: " + kills;

        Time.timeScale = 0f; // 終極魔法：時間暫停
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // 恢復時間流動 (不然重開會卡住)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 重新載入關卡
    }

    // ==========================================
    // 🎁 Boss 獎勵與流派選擇系統
    // ==========================================
    public void ShowReward(bool isBigBoss)
    {
        isBigBossReward = isBigBoss;
        rewardPanel.SetActive(true);

        Time.timeScale = 0f; // 時間暫停，讓玩家慢慢思考流派

        if (!isBigBossReward)
        {
            // 🌟 小 Boss：三個分支全部打開
            buttonCObject.SetActive(true);

            btnAText.text = "輕弩流派\n射速加快 (+50%)\n傷害降低 (-20%)";
            btnBText.text = "重砲流派\n攻擊加痛 (+15)\n攻速變慢 (-20%)";
            btnCText.text = "多重箭流派\n箭矢數量 +2\n傷害降低 (-50%)";
        }
        else
        {
            // 🌟 大 Boss：只有兩個分支，把第三個按鈕藏起來！
            buttonCObject.SetActive(false);

            btnAText.text = "王牌力量\n攻擊力 x 2 倍 !!";
            btnBText.text = "狂暴極速\n攻擊速度 x 2 倍 !!";
        }
    }


    // 玩家選擇 A 按鈕
    public void ChooseRewardA()
    {
        if (playerStats != null)
        {
            if (!isBigBossReward)
            {
                playerStats.attackSpeed += 1.5f; // 攻速變快
                playerStats.baseDamage *= 0.8f;  // 攻擊變弱
            }
            else
            {
                playerStats.baseDamage *= 2f;    // 攻擊力 x2
            }
        }
        ResumeGame();
    }

    // 玩家選擇 B 按鈕
    public void ChooseRewardB()
    {
        if (playerStats != null)
        {
            if (!isBigBossReward)
            {
                playerStats.baseDamage += 15f;   // 攻擊變痛
                playerStats.attackSpeed *= 0.8f; // 攻速變慢
            }
            else
            {
                playerStats.attackSpeed *= 2f;   // 攻速 x2
            }
        }
        ResumeGame();
    }

    // 玩家選擇 C 按鈕 (只有小 Boss 有這個選項)
    public void ChooseRewardC()
    {
        if (playerStats != null)
        {
            if (!isBigBossReward)
            {
                playerStats.arrowCount += 2;    // 箭矢數量增加！
                playerStats.baseDamage *= 0.5f; // 傷害降低
            }
        }
        ResumeGame();
    }

    // 領完獎勵，關閉面板並恢復遊戲時間
    private void ResumeGame()
    {
        rewardPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // 🌟 新增：召喚 Boss 的準備動作
    private void SpawnBoss(bool isBigBoss , float targetZ)
    {
        Transform playerTrans = playerStats.transform;

        // 1. 計算生成位置：生成在玩家前方 55 公尺 (在兩扇門的前方，不覆蓋門)
        Vector3 spawnPos = new Vector3(0, 1.5f, targetZ);

        // ==========================================
        // 🌟 2. 核心複製：套用小怪公式，計算 Boss 降落位置的小怪基準血量
        // ==========================================
        float baseEnemyMaxHp = 50f; // 對應 Enemy 腳本裡的初始 maxHp
        float scalingDistance = spawnPos.z - 30f;
        if (scalingDistance < 0) scalingDistance = 0;

        // 計算目前路段所在的階層
        float stage = Mathf.Floor(scalingDistance / 40f);
        // 計算目前的難度成長倍率
        float scaleFactor = Mathf.Pow(1.1f, stage);

        // 算出「如果此處生成一隻一般小怪，牠的血量會是多少」
        float currentEnemyHpAtThisPosition = Mathf.Round(baseEnemyMaxHp * scaleFactor);


        // 3. 準備實體化 Boss
        GameObject spawnedBoss = null;

        if (isBigBoss)
        {
            Debug.Log("🔥 警告！大 Boss 即將降臨！");
            spawnedBoss = Instantiate(bigBossPrefab, spawnPos, Quaternion.identity);

            BossHealth bossScript = spawnedBoss.GetComponent<BossHealth>();
            if (bossScript != null)
            {
                // 大 Boss 設定為該路段小怪血量的 5 倍
                float bigBossHp = currentEnemyHpAtThisPosition * 5f;
                bossScript.SetupHealth(bigBossHp);
            }
        }
        else
        {
            Debug.Log("⚠️ 準備戰鬥！小 Boss 出現！");
            spawnedBoss = Instantiate(smallBossPrefab, spawnPos, Quaternion.identity);

            BossHealth bossScript = spawnedBoss.GetComponent<BossHealth>();
            if (bossScript != null)
            {
                // 🌟 小 Boss 精準設定為該路段小怪血量的 2 倍！
                float smallBossHp = currentEnemyHpAtThisPosition * 2f;
                bossScript.SetupHealth(smallBossHp);
            }
        }
    }
}