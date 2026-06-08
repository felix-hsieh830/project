using UnityEngine;

public class ArrowFly : MonoBehaviour
{
    [Header("箭矢飛行速度")]
    public float speed = 15f;
    public float lifeTime = 3f;

    private float totalSpeed;
    private float finalDamage;
    private float yawDegreesPerSecond = 0f;
    private bool isSetup = false;
    private bool hasHit = false;
    private PlayerStats playerStats; // 🌟 抓取玩家狀態

    public void Setup(float playerDamage, float playerRange, float critRate, float critDamage, float inheritedSpeed, float flightSpeedMultiplier, float yawRate = 0f, PlayerStats ownerStats = null)
    {
        isSetup = true;
        playerStats = ownerStats != null ? ownerStats : FindAnyObjectByType<PlayerStats>(); // 🌟 啟動時取得玩家天賦

        if (Random.value <= critRate)
        {
            finalDamage = playerDamage * critDamage;
            transform.localScale *= 1.8f;
        }
        else
        {
            finalDamage = playerDamage;
        }

        totalSpeed = (speed * flightSpeedMultiplier) + inheritedSpeed;
        lifeTime = Mathf.Min(playerRange, 90f) / totalSpeed;
        Destroy(gameObject, lifeTime);
        yawDegreesPerSecond = yawRate;
    }

    void Start()
    {
        if (!isSetup) { totalSpeed = speed; Destroy(gameObject, lifeTime); }
    }

    void Update()
    {
        if (yawDegreesPerSecond != 0f) transform.Rotate(0f, yawDegreesPerSecond * Time.deltaTime, 0f);

        float stepDistance = totalSpeed * Time.deltaTime;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, stepDistance))
        {
            HandleCollision(hit.collider);
        }
        transform.Translate(0, 0, stepDistance);
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    // 🌟 將碰撞邏輯整合，方便套用天賦
    private void HandleCollision(Collider hitCollider)
    {
        if (hasHit) return;

        Enemy target = hitCollider.GetComponent<Enemy>();
        BossHealth boss = hitCollider.GetComponent<BossHealth>();

        if (target != null || boss != null)
        {
            hasHit = true;
            bool dealtDamage = false;

            if (target != null)
            {
                dealtDamage = target.TakeDamage(finalDamage);
            }
            else if (boss != null)
            {
                dealtDamage = boss.TakeDamage(finalDamage);
                // (你可以選擇未來讓Boss也中標ApplyPoison，這邊先只做普怪)
            }

            if (playerStats == null) playerStats = FindAnyObjectByType<PlayerStats>();

            if (dealtDamage && playerStats != null && playerStats.lifestealLevel > 0)
            {
                float damageForLifesteal = Mathf.Max(1f, finalDamage);
                float healAmount = damageForLifesteal * (playerStats.lifestealLevel * 0.05f);
                playerStats.Heal(Mathf.Max(1, Mathf.FloorToInt(healAmount))); // 至少回 1 滴
            }

            Destroy(gameObject); // 命中後銷毀箭矢
        }
    }
}
