using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("子彈設定")]
    public float speed = 15f;
    public int damage = 20;       // 打中玩家扣多少血
    public float lifeTime = 4f;   // 飛出畫面後幾秒銷毀，避免浪費效能

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 往自身的正前方 (Z 軸) 飛行
        transform.Translate(0, 0, speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        // 偵測是否打中玩家
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.TakeDamage(damage); // 玩家扣血
            Destroy(gameObject);       // 子彈銷毀
        }
    }
}