using UnityEngine;
using TMPro;

public class TreasureChest : MonoBehaviour
{
    [Header("寶箱基礎數值")]
    public int hpBonus = 30;                // 🌟 20 → 30
    public float damageBonus = 8f;          // 🌟 5 → 8
    public float attackSpeedBonus = 0.3f;   // 🌟 0.2 → 0.3
    public float attackRangeBonus = 1.5f; // 🌟 6 → 1.5
    public float critRateBonus = 0.05f;
    public float moveSpeedBonus = 0.5f;

    [Header("UI 顯示")]
    public TextMeshPro rewardText;

    private int selectedReward;
    private PlayerStats player;
    private PlayerMove playerMove;

    void Start()
    {
        selectedReward = Random.Range(0, 6);
        player = FindAnyObjectByType<PlayerStats>();
        if (player != null) playerMove = player.GetComponent<PlayerMove>();

        // 🌟 距離成長：每 40m 一階段，每階段 +12%
        float scalingDistance = Mathf.Max(0, transform.position.z - 30f);
        float stage = Mathf.Floor(scalingDistance / 40f);
        float distanceMultiplier = 1f + stage * 0.12f;

        hpBonus = Mathf.RoundToInt(hpBonus * distanceMultiplier);
        damageBonus = (float)System.Math.Round(damageBonus * distanceMultiplier, 1);
        attackSpeedBonus = (float)System.Math.Round(attackSpeedBonus * distanceMultiplier, 2);
        attackRangeBonus = (float)System.Math.Round(attackRangeBonus * distanceMultiplier, 1);
        critRateBonus = (float)System.Math.Round(critRateBonus * distanceMultiplier, 2);
        moveSpeedBonus = (float)System.Math.Round(moveSpeedBonus * distanceMultiplier, 2);

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
            FloatingTextSpawner.instance?.Spawn(floatingMsg, transform.position, Color.green, Vector3.up, other.transform);

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