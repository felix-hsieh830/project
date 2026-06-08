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

    public void Setup(float playerDamage, float playerRange, float critRate, float critDamage,
                  float inheritedSpeed, float flightSpeedMultiplier, float yawRate = 0f,
                  PlayerStats stats = null)
    {
        isSetup = true;
        playerStats = stats;
        //playerStats = ownerStats != null ? ownerStats : FindAnyObjectByType<PlayerStats>(); // 🌟 啟動時取得玩家天賦

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

            if (target != null) target.TakeDamage(finalDamage);
            else if (boss != null) boss.TakeDamage(finalDamage);

            if (playerStats == null) playerStats = FindAnyObjectByType<PlayerStats>();

            // 直接觸發，不依賴 dealtDamage
            if (playerStats != null && playerStats.lifestealLevel > 0)
            {
                float healAmount = Mathf.Max(1f, finalDamage) * (playerStats.lifestealLevel * 0.05f);
                playerStats.Heal(Mathf.Max(1, Mathf.FloorToInt(healAmount)));
            }

            Destroy(gameObject);
        }
    }
}
