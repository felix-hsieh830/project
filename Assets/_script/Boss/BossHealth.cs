using UnityEngine;
using TMPro;
using System.Collections; // 🌟 記得加這行才能用時間等待魔法

public class BossHealth : MonoBehaviour
{
    [Header("Boss 數值設定")]
    public float hp = 1000f;
    public float currentHp;
    public bool isBigBoss = false;
    public float bigBossEngageDistance = 32f;

    [Header("UI 顯示")]
    public TextMeshPro hpText;

    private bool isDead = false;
    public bool isInvincible = false; // 🌟 新增：無敵狀態開關

    public void SetupHealth(float newHP)
    {
        hp = newHP;
        currentHp = hp;

        // 🌟 新增防盲狙機制：只要是大 Boss，一出生就套上無敵護盾！
        // 這樣牠在畫面外乖乖等你的時候，就不會被流彈打死了。
        if (isBigBoss)
        {
            isInvincible = true;
        }

        UpdateHPUI(); 
    }

    void Update()
    {
        if (isBigBoss && !isDead)
        {
            PlayerMove playerMove = FindAnyObjectByType<PlayerMove>();
            if (playerMove != null && !playerMove.isFightingBigBoss)
            {
                float distanceToPlayer = transform.position.z - playerMove.transform.position.z;

                if (distanceToPlayer <= bigBossEngageDistance && distanceToPlayer > 0)
                {
                    playerMove.isFightingBigBoss = true; // 強制玩家停車

                    // 🌟 1. Boss 進入登場無敵狀態
                    isInvincible = true;
                    FindAnyObjectByType<LevelManager>()?.BeginBigBossArena(transform.position.z);

                    // 🌟 2. 呼叫攝影機往下沉
                    if (Camera.main != null)
                    {
                        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
                        if (cam != null) cam.SwitchToBossCamera();
                    }

                    // 🌟 3. 啟動登場倒數計時
                    StartCoroutine(BossIntroRoutine());
                }
            }
        }
    }


 
    // 🌟 登場過場動畫協程
    IEnumerator BossIntroRoutine()
    {
        Debug.Log("🎥 大 Boss 登場運鏡中...無敵狀態！");

        yield return new WaitForSeconds(2.0f);

        yield return new WaitForSeconds(0.5f);

        isInvincible = false; // 解除無敵
        Debug.Log("⚔️ 戰鬥正式開始！");
    }

    public bool TakeDamage(float damage)
    {
        if (isInvincible || isDead) return false;
        currentHp -= damage;

        UpdateHPUI(); // 更新血條 UI

        if (currentHp <= 0)
        {
            Die();
        }

        return true;
    }

    void UpdateHPUI()
    {
        if (hpText == null) return;
        if (currentHp < 0) currentHp = 0;
        hpText.text = Mathf.CeilToInt(currentHp).ToString();

        if (isBigBoss) hpText.color = Color.red;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player != null)
        {
            isDead = true;
            int damageToPlayer = Mathf.RoundToInt(currentHp);
            player.TakeDamage(damageToPlayer);

            if (isBigBoss)
            {
                EndBigBossCleanup();
            }

            if (player.currentHp > 0)
            {
                if (isBigBoss)
                {
                    PlayerMove playerMove = player.GetComponent<PlayerMove>();
                    if (playerMove != null) playerMove.isFightingBigBoss = false;

                    // 🌟 撞死 Boss 也要把鏡頭拉回空中
                    if (Camera.main != null) Camera.main.GetComponent<CameraFollow>()?.SwitchToNormalCamera();
                }

                // GameManager gm = FindAnyObjectByType<GameManager>();
                // if (gm != null) gm.ShowReward(isBigBoss);
            }
            Destroy(gameObject);
        }
    }

    private void Die()
    {
        isDead = true;

        if (isBigBoss)
        {
            PlayerMove playerMove = FindAnyObjectByType<PlayerMove>();
            if (playerMove != null) playerMove.isFightingBigBoss = false;
            EndBigBossCleanup();
            if (Camera.main != null) Camera.main.GetComponent<CameraFollow>()?.SwitchToNormalCamera();
        }

        // 🌟 改成在 Boss 位置生成獎勵箱，不直接跳獎勵介面
        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.EndEnemyPlusOneStage();
            gm.SpawnBossRewardChest(transform.position, isBigBoss);
        }

        Destroy(gameObject);
    }

    private void EndBigBossCleanup()
    {
        FindAnyObjectByType<LevelManager>()?.EndBigBossArena();
    }

    public string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.##") + "B";
        else if (number >= 1000000) return (number / 1000000f).ToString("0.##") + "M";
        else if (number >= 1000) return (number / 1000f).ToString("0.##") + "K";

        return Mathf.FloorToInt(number).ToString();
    }
}
