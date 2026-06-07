using UnityEngine;
using TMPro;

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
    public TextMeshPro rewardText;

    private int selectedReward;
    private PlayerStats player; // 🌟 為了磁鐵功能抓取玩家

    void Start()
    {
        selectedReward = Random.Range(0, 6);
        player = FindAnyObjectByType<PlayerStats>();

        if (rewardText != null)
        {
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

    void Update()
    {
        // 🌟 實裝金幣磁鐵功能
        if (player != null && player.magnetLevel > 0)
        {
            float magnetRadius = player.magnetLevel * 5f;
            float distance = Vector3.Distance(transform.position, player.transform.position);

            // 如果在吸附範圍內，就用 MoveTowards 魔法飛向主角！
            if (distance <= magnetRadius)
            {
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, 15f * Time.deltaTime);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerStats stats = other.GetComponent<PlayerStats>();
        PlayerMove moveScript = other.GetComponent<PlayerMove>();

        if (stats != null)
        {
            string floatingMsg = rewardText.text.Replace("\n", " ");
            FloatingTextSpawner.instance?.Spawn(floatingMsg, transform.position, Color.green);

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
            Destroy(gameObject);
        }
    }
}