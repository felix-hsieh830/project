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

    private int maxShotBurstsPerFrame = 4;

    // 箭矢基礎速度，要跟 ArrowFly 裡的 speed 一致
    private float arrowBaseSpeed = 15f;
    private const float inheritedForwardSpeedScale = 0.35f;

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

        float actualAttackSpeed = Mathf.Max(0.1f, stats.attackSpeed);
        float fireCooldown = 1f / actualAttackSpeed;

        // 🌟 同步動畫速度倍率
        if (animController != null && animController.animator != null)
        {
            float speedMultiplier = stats.attackSpeed / 1f; // 1f 是初始攻速
            animController.animator.SetFloat("AttackSpeedParam", speedMultiplier);
        }

        int shotBursts = 0;
        while (timer >= fireCooldown && shotBursts < maxShotBurstsPerFrame)
        {
            Shoot(actualAttackSpeed);
            timer -= fireCooldown;
            shotBursts++;
        }

        if (shotBursts == maxShotBurstsPerFrame && timer >= fireCooldown)
        {
            timer = 0f;
        }
    }

    void Shoot(float effectiveAttackSpeed)
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("請在 Inspector 中指派 Arrow Prefab！");
            return;
        }

        int actualArrowCount = Mathf.Max(1, stats.arrowCount);
        float flightSpeedMultiplier = GetFlightSpeedMultiplier(effectiveAttackSpeed);
        float inheritedArrowSpeed = currentPlayerZSpeed * inheritedForwardSpeedScale;

        float actualRange = Mathf.Min(stats.attackRange, 90f);
        float estimatedTotalSpeed = arrowBaseSpeed * flightSpeedMultiplier + inheritedArrowSpeed;
        float flightTime = actualRange / Mathf.Max(estimatedTotalSpeed, 0.1f);

        float anglePerStep = 1.5f;   // 每根箭的生成角度間隔
        float yawPerStep = 4f;   // 越外側的箭飛行中額外擴散角速度

        for (int i = 0; i < actualArrowCount; i++)
        {
            float center = (actualArrowCount - 1) / 2f;
            float distFromCenter = i - center; // 負 = 左，正 = 右
            float maxAngleLimit = 100f;

            float spawnAngle = Mathf.Clamp(distFromCenter * anglePerStep, -maxAngleLimit, maxAngleLimit);
            float yawRate = (distFromCenter / Mathf.Max(center, 1f)) * yawPerStep / Mathf.Max(flightTime, 0.1f);

            Quaternion arrowRotation = transform.rotation * Quaternion.Euler(0, spawnAngle, 0);
            Vector3 spawnPosition = transform.position + new Vector3(distFromCenter * 0.18f, 1.2f, 1.5f - Mathf.Abs(distFromCenter) * 0.08f);
            GameObject arrow = Instantiate(arrowPrefab, spawnPosition, arrowRotation);

            ArrowFly arrowScript = arrow.GetComponent<ArrowFly>();
            if (arrowScript != null)
            {
                float finalBaseDamage = stats.baseDamage;
                arrowScript.ApplyVisualColor(stats.GetArrowColor(), stats.arrowWoodColor, stats.GetArrowEmissionColor(), stats.arrowWoodEmissionColor, true);
                arrowScript.Setup(finalBaseDamage, stats.attackRange, stats.critRate, stats.critDamage, inheritedArrowSpeed, flightSpeedMultiplier, yawRate, stats);
            }
        }

        SfxManager.Play("arrow_shoot", 0.42f, 0.045f);

        // 🌟 通知動畫控制器播放射擊動畫
        if (animController != null)
            animController.TriggerShootAnimation();
    }

    private float GetFlightSpeedMultiplier(float effectiveAttackSpeed)
    {
        float speedBonus = Mathf.Max(0f, effectiveAttackSpeed - 1f) * 0.16f;
        return Mathf.Clamp(1f + speedBonus, 1f, 2.4f);
    }
}
