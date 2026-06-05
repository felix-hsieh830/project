using UnityEngine;
using TMPro; // 🌟 記得加這行才能控制文字

public class TreasureChest : MonoBehaviour
{
    [Header("寶箱數值設定")]
    public int hpBonus = 20;
    public float damageBonus = 5f;
    public float attackSpeedBonus = 0.2f;
    public float attackRangeBonus = 5f;
    public float critRateBonus = 0.05f;
    public float moveSpeedBonus = 0.5f;

    [Header("UI 顯示")]
    public TextMeshPro rewardText; // 🌟 用來裝 3D 文字的格子

    private int selectedReward; // 🌟 記憶體：記住這個寶箱抽到什麼

    void Start()
    {
        // ==========================================
        // 🌟 改變邏輯：在生出來的瞬間，就決定好獎勵！
        // ==========================================
        selectedReward = Random.Range(0, 6);

        if (rewardText != null)
        {
            // 根據抽到的數字，把對應的文字寫在寶箱頭上！
            // "\n" 是換行的意思，可以讓排版更漂亮
            switch (selectedReward)
            {
                case 0: rewardText.text = "最大生命\n+" + hpBonus; break;
                case 1: rewardText.text = "攻擊力\n+" + damageBonus; break;
                case 2: rewardText.text = "攻擊速度\n+" + attackSpeedBonus; break;
                case 3: rewardText.text = "攻擊距離\n+" + attackRangeBonus; break;
                case 4: rewardText.text = "爆擊機率\n+" + (critRateBonus * 100) + "%"; break;
                case 5: rewardText.text = "橫移速度\n+" + moveSpeedBonus; break;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerStats stats = other.GetComponent<PlayerStats>();
        PlayerMove moveScript = other.GetComponent<PlayerMove>();

        if (stats != null)
        {
            // 🌟 修正飄字：直接抓寶箱上的字，並把換行(\n)換成空格
            string floatingMsg = rewardText.text.Replace("\n", " ");
            FloatingTextSpawner.instance?.Spawn(floatingMsg, transform.position, Color.green);

            // 🌟 撞到時，把剛剛 Start 裡記住的獎勵發給玩家
            switch (selectedReward)
            {
                case 0: stats.AddMaxHealth(hpBonus); break;
                case 1: stats.baseDamage += damageBonus; break;
                case 2: stats.attackSpeed += attackSpeedBonus; break;
                case 3: stats.attackRange += attackRangeBonus; break;
                case 4: stats.critRate += critRateBonus; break;
                case 5:
                    if (moveScript != null)
                    {
                        moveScript.horizontalSpeed += moveSpeedBonus;
                        if (moveScript.horizontalSpeed > 12f) moveScript.horizontalSpeed = 12f;
                    }
                    break;
            }
            
            // 獎勵給完，銷毀寶箱
            Destroy(gameObject);
        }
    }
}