using UnityEngine;

public class ArrowFly : MonoBehaviour
{
    [Header("箭矢飛行速度")]
    public float speed = 15f;

    [Header("自動銷毀時間（未呼叫 Setup 時的備用）")]
    public float lifeTime = 3f;

    private float totalSpeed;
    private float finalDamage;
    private float yawDegreesPerSecond = 0f;
    private bool isSetup = false;

    public void Setup(float playerDamage, float playerRange, float critRate, float critDamage, float inheritedSpeed, float flightSpeedMultiplier, float yawRate = 0f)
    {
        isSetup = true;

        // 爆擊判定
        if (Random.value <= critRate)
        {
            finalDamage = playerDamage * critDamage;
            transform.localScale *= 1.8f;
        }
        else
        {
            finalDamage = playerDamage;
        }

        float baseSpeed = speed * flightSpeedMultiplier;
        totalSpeed = baseSpeed + inheritedSpeed;

        // 依射程計算存活時間
        float finalRange = Mathf.Min(playerRange, 90f);
        lifeTime = finalRange / totalSpeed;
        Destroy(gameObject, lifeTime);

        yawDegreesPerSecond = yawRate;
    }

    void Start()
    {
        // 如果沒有呼叫 Setup（例如直接拖放測試），使用預設 lifeTime 和 speed
        if (!isSetup)
        {
            totalSpeed = speed;
            Destroy(gameObject, lifeTime);
            Debug.Log("⚠️ 箭矢未呼叫 Setup，使用預設值飛行");
        }
    }

    void Update()
    {
        // 旋轉箭矢（多重箭流派等需要偏轉時才會有值）
        if (yawDegreesPerSecond != 0f)
        {
            transform.Rotate(0f, yawDegreesPerSecond * Time.deltaTime, 0f);
        }

        float stepDistance = totalSpeed * Time.deltaTime;

        // Raycast 優先偵測碰撞（防止高速穿透）
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, stepDistance))
        {
            Enemy target = hit.collider.GetComponent<Enemy>();
            if (target != null)
            {
                target.TakeDamage(finalDamage);
                Destroy(gameObject);
                return;
            }
            BossHealth boss = hit.collider.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(finalDamage);
                Destroy(gameObject);
                return;
            }
        }

        // 移動箭矢（沿自身 Z 軸前進）
        transform.Translate(0, 0, stepDistance);
    }

    // Trigger 作為 Raycast 的補強（處理範圍型碰撞體）
    void OnTriggerEnter(Collider other)
    {
        Enemy target = other.GetComponent<Enemy>();
        if (target != null)
        {
            target.TakeDamage(finalDamage);
            Destroy(gameObject);
            return;
        }
        BossHealth boss = other.GetComponent<BossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(finalDamage);
            Destroy(gameObject);
            return;
        }
    }
}