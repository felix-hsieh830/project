using UnityEngine;

public class BigBossAI : MonoBehaviour
{
    [Header("階段一：移動設定 (滑步)")]
    public float moveSpeed = 4f;
    public float limitX = 4f;
    private int moveDirection = 1;

    [Header("階段一：攻擊設定 (雷射)")]
    public GameObject laserPrefab;
    public float fireRate = 1.5f;
    private float fireTimer = 0f;

    [Header("階段二：死靈召喚設定")]
    public GameObject minionPrefab;   // 🌟 準備用來召喚的小怪
    public float summonRate = 3f;     // 每隔幾秒召喚一波
    private float summonTimer = 0f;

    private BossHealth healthScript;
    private bool isActive = false;
    private bool isPhaseTwo = false;  // 🌟 紀錄是否進入第二階段

    void Start()
    {
        healthScript = GetComponent<BossHealth>();
    }

    void Update()
    {
        // Boss 死了就不動
        if (healthScript != null && healthScript.hp <= 0) return;

        if (healthScript != null && healthScript.isInvincible) return;

        // 偵測玩家是否就位
        if (!isActive)
        {
            PlayerMove player = FindAnyObjectByType<PlayerMove>();
            if (player != null && player.isFightingBigBoss)
            {
                isActive = true;
            }
            return;
        }

        // ==========================================
        // 🌟 核心：判斷是否該進入第二階段 (血量 <= 50%)
        // ==========================================
        // 在你的 BossHealth 裡，currentHp 裝的是最大血量，hp 才是當前血量
        if (!isPhaseTwo && healthScript.hp <= healthScript.currentHp * 0.5f)
        {
            isPhaseTwo = true;
            Debug.Log("💀 大 Boss 進入第二階段：死靈召喚！");

            // 變身時，強制把 Boss 瞬移回正中間，比較有魔王的氣勢
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        }

        // ==========================================
        // 執行階段行為
        // ==========================================
        if (!isPhaseTwo)
        {
            // 【第一階段】左右滑步 + 射雷射
            transform.Translate(Vector3.right * moveDirection * moveSpeed * Time.deltaTime);

            if (transform.position.x >= limitX)
            {
                moveDirection = -1;
                transform.position = new Vector3(limitX, transform.position.y, transform.position.z);
            }
            else if (transform.position.x <= -limitX)
            {
                moveDirection = 1;
                transform.position = new Vector3(-limitX, transform.position.y, transform.position.z);
            }

            fireTimer += Time.deltaTime;
            if (fireTimer >= fireRate)
            {
                ShootLaser();
                fireTimer = 0f;
            }
        }
        else
        {
            // 【第二階段】站在原地 + 召喚死靈
            summonTimer += Time.deltaTime;
            if (summonTimer >= summonRate)
            {
                SummonMinions();
                summonTimer = 0f;
            }
        }
    }

    void ShootLaser()
    {
        if (laserPrefab != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, 1f, -1.5f);
            Instantiate(laserPrefab, spawnPos, Quaternion.Euler(0, 180, 0));
        }
    }

    void SummonMinions()
    {
        if (minionPrefab != null)
        {
            // 🌟 一次在 Boss 的左前與右前方各召喚一隻小怪！
            Vector3 leftPos = transform.position + new Vector3(-2f, -1f, -3f);
            Vector3 rightPos = transform.position + new Vector3(2f, -1f, -3f);

            Instantiate(minionPrefab, leftPos, Quaternion.identity);
            Instantiate(minionPrefab, rightPos, Quaternion.identity);
        }
    }
}