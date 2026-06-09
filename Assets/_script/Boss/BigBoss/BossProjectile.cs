using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 15f;
    public float damagePercent = 0.2f; // 打中玩家扣最大生命比例
    public float lifeTime = 4f;   // 飛出畫面後幾秒銷毀，避免浪費效能
    private bool hasHit;
    private const float HitRadius = 0.75f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 往自身的正前方 (Z 軸) 飛行
        Vector3 direction = transform.forward;
        float moveDistance = speed * Time.deltaTime;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, HitRadius, direction, out hit, moveDistance, ~0, QueryTriggerInteraction.Collide))
        {
            if (TryHitPlayer(hit.collider))
            {
                return;
            }
        }

        transform.position += direction * moveDistance;
    }

    void OnTriggerEnter(Collider other)
    {
        TryHitPlayer(other);
    }

    private bool TryHitPlayer(Collider other)
    {
        if (hasHit) return false;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return false;

        hasHit = true;
        int damage = Mathf.Max(1, Mathf.CeilToInt(player.maxHp * damagePercent));
        player.TakeDamage(damage); // 玩家扣血
        Destroy(gameObject);       // 子彈銷毀
        return true;
    }
}
