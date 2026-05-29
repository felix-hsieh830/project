using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject arrowPrefab;

    private float timer = 0f;
    private PlayerStats stats;

    private float currentPlayerZSpeed = 0f;
    private Vector3 lastPosition;

    // 🌟 減負系統的極限設定 (可自由微調)
    private float maxAttackSpeed = 80f; // 一秒最多射 10 次 (冷卻 0.1 秒)
    private int maxArrowsPerShot = 100;  // 一次最多射出 15 支實體箭

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Time.deltaTime > 0f)
        {
            currentPlayerZSpeed = (transform.position.z - lastPosition.z) / Time.deltaTime;
        }
        else
        {
            currentPlayerZSpeed = 0f;
        }

        lastPosition = transform.position;
        timer += Time.deltaTime;

        // ==========================================
        // 🌟 1. 攻速濃縮判定
        // ==========================================
        float actualAttackSpeed = stats.attackSpeed;
        float speedDamageMultiplier = 1f;

        // 如果玩家攻速狂飆超過極限，就限制住，並把溢出的攻速轉成傷害倍率
        if (actualAttackSpeed > maxAttackSpeed)
        {
            speedDamageMultiplier = actualAttackSpeed / maxAttackSpeed;
            actualAttackSpeed = maxAttackSpeed;
        }

        float fireCooldown = 1f / actualAttackSpeed;

        if (timer >= fireCooldown)
        {
            // 把攻速換算來的傷害倍率傳遞給射擊功能
            Shoot(speedDamageMultiplier);
            timer = 0f;
        }
    }

    void Shoot(float speedDamageMultiplier)
    {
        int actualArrowCount = stats.arrowCount;
        float countDamageMultiplier = 1f;

        if (actualArrowCount > maxArrowsPerShot)
        {
            countDamageMultiplier = (float)actualArrowCount / maxArrowsPerShot;
            actualArrowCount = maxArrowsPerShot;
        }

        float totalDamageMultiplier = speedDamageMultiplier * countDamageMultiplier;
        float spreadAngle = 25f;
        float flightSpeedMultiplier = 1f + (stats.attackSpeed * 0.1f);

        for (int i = 0; i < actualArrowCount; i++)
        {
            Quaternion arrowRotation = transform.rotation;
            if (actualArrowCount > 1)
            {
                float angleOffset = -spreadAngle / 2f + (spreadAngle / (actualArrowCount - 1) * i);
                arrowRotation *= Quaternion.Euler(0, angleOffset, 0);
            }

            Vector3 spawnPosition = transform.position + new Vector3(0, 0, 1.5f);
            GameObject arrow = Instantiate(arrowPrefab, spawnPosition, arrowRotation);

            ArrowFly arrowScript = arrow.GetComponent<ArrowFly>();
            if (arrowScript != null)
            {
                float finalBaseDamage = stats.baseDamage * totalDamageMultiplier;

                // 🌟 把算好的 flightSpeedMultiplier 塞進最後一個參數傳出去！
                arrowScript.Setup(finalBaseDamage, stats.attackRange, stats.critRate, stats.critDamage, currentPlayerZSpeed, flightSpeedMultiplier);
            }
        }
    }
}