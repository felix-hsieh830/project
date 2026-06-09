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
    private PlayerStats playerStats;

    public void Setup(
        float playerDamage,
        float playerRange,
        float critRate,
        float critDamage,
        float inheritedSpeed,
        float flightSpeedMultiplier,
        float yawRate = 0f,
        PlayerStats stats = null
    )
    {
        isSetup = true;
        playerStats = stats;

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

        if (totalSpeed <= 0f)
        {
            totalSpeed = speed;
        }

        lifeTime = Mathf.Min(playerRange, 90f) / totalSpeed;
        Destroy(gameObject, lifeTime);

        yawDegreesPerSecond = yawRate;
    }

    void Start()
    {
        if (!isSetup)
        {
            totalSpeed = speed;
            Destroy(gameObject, lifeTime);
        }
    }

    void Update()
    {
        if (yawDegreesPerSecond != 0f)
        {
            transform.Rotate(0f, yawDegreesPerSecond * Time.deltaTime, 0f);
        }

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

    private void HandleCollision(Collider hitCollider)
    {
        if (hasHit)
        {
            return;
        }

        // 重點：改成抓父物件，避免 Collider 在子物件時抓不到 Enemy
        Enemy target = hitCollider.GetComponentInParent<Enemy>();
        BossHealth boss = hitCollider.GetComponentInParent<BossHealth>();

        if (target == null && boss == null)
        {
            return;
        }

        hasHit = true;

        bool dealtDamage = false;
        float lifestealDamage = 0f;

        if (target != null)
        {
            lifestealDamage = Mathf.Min(finalDamage, Mathf.Max(0f, target.CurrentHp));
            dealtDamage = target.TakeDamage(finalDamage);
        }
        else if (boss != null)
        {
            lifestealDamage = Mathf.Min(finalDamage, Mathf.Max(0f, boss.currentHp));
            dealtDamage = boss.TakeDamage(finalDamage);
        }

        if (!dealtDamage)
        {
            Destroy(gameObject);
            return;
        }

        if (playerStats == null)
        {
            playerStats = FindAnyObjectByType<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogWarning("吸血失敗：找不到 PlayerStats");
            Destroy(gameObject);
            return;
        }

        if (playerStats.lifestealLevel <= 0)
        {
            Debug.Log("沒有觸發吸血：lifestealLevel 目前是 0");
            Destroy(gameObject);
            return;
        }

        float lifestealRate = playerStats.lifestealLevel * 0.1f;
        if (lifestealDamage <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float healAmountFloat = lifestealDamage * lifestealRate;
        int healAmount = Mathf.Max(1, Mathf.FloorToInt(healAmountFloat));

        //Debug.Log("觸發吸血，等級：" + playerStats.lifestealLevel + "，回血量：" + healAmount);

        playerStats.Heal(healAmount, true);

        Destroy(gameObject);
    }
}
