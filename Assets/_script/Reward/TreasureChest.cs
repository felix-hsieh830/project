using UnityEngine;
using TMPro;

public class TreasureChest : MonoBehaviour
{
    [Header("寶箱基礎數值")]
    public int hpBonus = 20;
    public float damageBonus = 4.5f;
    public float attackSpeedBonus = 0.18f;
    public float attackRangeBonus = 1.0f;
    public float critRateBonus = 0.005f;
    public float moveSpeedBonus = 0.35f;

    [Header("UI 顯示")]
    public TextMeshPro rewardText;

    private int selectedReward;
    private PlayerStats player;
    private PlayerMove playerMove;

    void Start()
    {
        selectedReward = RollRewardIndex();
        player = FindAnyObjectByType<PlayerStats>();
        if (player != null) playerMove = player.GetComponent<PlayerMove>();

        float scalingDistance = Mathf.Max(0, transform.position.z - 30f);
        float stage = Mathf.Floor(scalingDistance / 80f);
        float distanceMultiplier = 1f + stage * 0.06f;

        hpBonus = Mathf.RoundToInt(hpBonus * distanceMultiplier);
        damageBonus = (float)System.Math.Round(damageBonus * distanceMultiplier, 1);
        attackSpeedBonus = (float)System.Math.Round(attackSpeedBonus * distanceMultiplier, 2);
        attackRangeBonus = 1f;
        critRateBonus = (float)System.Math.Round(critRateBonus * distanceMultiplier, 3);
        moveSpeedBonus = (float)System.Math.Round(moveSpeedBonus * distanceMultiplier, 2);

        if (rewardText != null)
        {
            switch (selectedReward)
            {
                case 0: rewardText.text = "最大生命\n+" + hpBonus; break;
                case 1: rewardText.text = "攻擊力\n+" + damageBonus; break;
                case 2: rewardText.text = "攻擊速度\n+" + attackSpeedBonus; break;
                case 3: rewardText.text = "攻擊距離\n+" + attackRangeBonus; break;
                case 4: rewardText.text = "爆擊機率\n+" + FormatPercent(critRateBonus); break;
                case 5: rewardText.text = "橫移速度\n+" + moveSpeedBonus; break;
            }
        }
    }

    private int RollRewardIndex()
    {
        int roll = Random.Range(0, 100);
        if (roll < 24) return 1; // 攻擊力
        if (roll < 46) return 2; // 攻擊速度
        if (roll < 62) return 3; // 攻擊距離
        if (roll < 78) return 0; // 最大生命
        if (roll < 92) return 4; // 爆擊機率
        return 5; // 橫移速度
    }

    private string FormatPercent(float value)
    {
        return (value * 100f).ToString("0.#") + "%";
    }

    void Update()
    {
        if (player != null && player.magnetLevel > 0)
        {
            float magnetRadius = player.magnetLevel * 3f;
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance <= magnetRadius)
            {
                float playerForwardSpeed = playerMove != null ? playerMove.forwardSpeed : 0f;
                float magnetSpeed = 15f + playerForwardSpeed + (distance * 2f);
                transform.position = Vector3.MoveTowards(transform.position, player.transform.position, magnetSpeed * Time.deltaTime);
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
            FloatingTextSpawner.instance?.Spawn(floatingMsg, other.transform.position, Color.green, Vector3.up, other.transform, 0.65f, 2.35f);

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
                        if (moveScript.horizontalSpeed > moveScript.maxHorizontalSpeed)
                        {
                            moveScript.horizontalSpeed = moveScript.maxHorizontalSpeed;
                        }
                    }
                    break;
            }
            Destroy(gameObject);
        }
    }
}
