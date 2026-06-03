using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject arrowPrefab;

    private float timer = 0f;
    private PlayerStats stats;

    private float currentPlayerZSpeed = 0f;
    private Vector3 lastPosition;

    private float maxAttackSpeed = 80f;
    private int maxArrowsPerShot = 100;

    // 🌟 箭矢基礎速度，要跟 ArrowFly 裡的 speed 一致
    private float arrowBaseSpeed = 15f;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Time.deltaTime > 0f)
            currentPlayerZSpeed = (transform.position.z - lastPosition.z) / Time.deltaTime;
        else
            currentPlayerZSpeed = 0f;

        lastPosition = transform.position;
        timer += Time.deltaTime;

        float actualAttackSpeed = stats.attackSpeed;
        float speedDamageMultiplier = 1f;

        if (actualAttackSpeed > maxAttackSpeed)
        {
            speedDamageMultiplier = actualAttackSpeed / maxAttackSpeed;
            actualAttackSpeed = maxAttackSpeed;
        }

        float fireCooldown = 1f / actualAttackSpeed;

        if (timer >= fireCooldown)
        {
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
        float flightSpeedMultiplier = 1f + (stats.attackSpeed * 0.1f);

        float actualRange = Mathf.Min(stats.attackRange, 90f);
        float estimatedTotalSpeed = arrowBaseSpeed * flightSpeedMultiplier + currentPlayerZSpeed;
        float flightTime = actualRange / Mathf.Max(estimatedTotalSpeed, 0.1f);

        // 🌟 每往外一格，增加幾度（這個是核心參數）
        float anglePerStep = 1f;

        // 🌟 越外側的箭飛行中額外擴散幾度
        float yawPerStep = 2.5f;

        for (int i = 0; i < actualArrowCount; i++)
        {
            // 🌟 算出這根箭距離中心幾格
            // 例如 5 根：索引 0~4，中心 = 2，距離 = -2,-1,0,1,2
            float center = (actualArrowCount - 1) / 2f;
            float distFromCenter = i - center; // 負 = 左，正 = 右
            float maxAngleLimit = 100f;

            // 🌟 角度直接用距離 * 固定步進，中間永遠是 0
            float spawnAngle = Mathf.Clamp(distFromCenter * anglePerStep, -maxAngleLimit, maxAngleLimit);

            // 🌟 越外側飛行中擴散越多
            float yawRate = (distFromCenter / Mathf.Max(center, 1f)) * yawPerStep / Mathf.Max(flightTime, 0.1f);

            Quaternion arrowRotation = transform.rotation * Quaternion.Euler(0, spawnAngle, 0);
            Vector3 spawnPosition = transform.position + new Vector3(0, 0, 1.5f);
            GameObject arrow = Instantiate(arrowPrefab, spawnPosition, arrowRotation);

            ArrowFly arrowScript = arrow.GetComponent<ArrowFly>();
            if (arrowScript != null)
            {
                float finalBaseDamage = stats.baseDamage * totalDamageMultiplier;
                arrowScript.Setup(finalBaseDamage, stats.attackRange, stats.critRate, stats.critDamage, currentPlayerZSpeed, flightSpeedMultiplier, yawRate);
            }
        }
    }
}