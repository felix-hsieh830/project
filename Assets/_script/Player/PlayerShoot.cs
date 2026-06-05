using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("射擊設定")]
    public GameObject arrowPrefab;

    [Header("初始射速設定（動畫用）")]
    public float baseFireRate = 0.5f;     // 基準發射速度，用來計算動畫倍率

    [Header("引用其他組件")]
    public PlayerAnimatorController animController;

    private float timer = 0f;
    private PlayerStats stats;

    private float currentPlayerZSpeed = 0f;
    private Vector3 lastPosition;

    private float maxAttackSpeed = 80f;
    private int maxArrowsPerShot = 100;

    // 箭矢基礎速度，要跟 ArrowFly 裡的 speed 一致
    private float arrowBaseSpeed = 15f;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        lastPosition = transform.position;

        if (animController == null)
            animController = GetComponent<PlayerAnimatorController>();
        if (stats == null)
            Debug.LogError("❌ 找不到 PlayerStats！請確認 PlayerStats 腳本掛在同一個物件上！");
    }

    void Update()
    {
        // 計算玩家當前 Z 軸移動速度（繼承給箭矢用）
        if (Time.deltaTime > 0f)
            currentPlayerZSpeed = (transform.position.z - lastPosition.z) / Time.deltaTime;
        else
            currentPlayerZSpeed = 0f;

        lastPosition = transform.position;
        timer += Time.deltaTime;

        float actualAttackSpeed = stats.attackSpeed;
        float speedDamageMultiplier = 1f;

        // 超過上限時，把多出來的攻速折算成傷害倍率
        if (actualAttackSpeed > maxAttackSpeed)
        {
            speedDamageMultiplier = actualAttackSpeed / maxAttackSpeed;
            actualAttackSpeed = maxAttackSpeed;
        }

        float fireCooldown = 1f / actualAttackSpeed;

        // 🌟 同步動畫速度倍率
        if (animController != null && animController.animator != null)
        {
            float speedMultiplier = stats.attackSpeed / 1f; // 1f 是初始攻速
            animController.animator.SetFloat("AttackSpeedParam", speedMultiplier);
        }

        if (timer >= fireCooldown)
        {
            Shoot(speedDamageMultiplier);
            timer = 0f;
        }
    }

    void Shoot(float speedDamageMultiplier)
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("請在 Inspector 中指派 Arrow Prefab！");
            return;
        }

        int actualArrowCount = stats.arrowCount;
        float countDamageMultiplier = 1f;

        // 超過上限時，把多出來的箭數折算成傷害倍率
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

        float anglePerStep = 1f;   // 每根箭的生成角度間隔
        float yawPerStep = 2.5f;   // 越外側的箭飛行中額外擴散角速度

        for (int i = 0; i < actualArrowCount; i++)
        {
            float center = (actualArrowCount - 1) / 2f;
            float distFromCenter = i - center; // 負 = 左，正 = 右
            float maxAngleLimit = 100f;

            float spawnAngle = Mathf.Clamp(distFromCenter * anglePerStep, -maxAngleLimit, maxAngleLimit);
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

        // 🌟 通知動畫控制器播放射擊動畫
        if (animController != null)
            animController.TriggerShootAnimation();
    }
}