using UnityEngine;

public class ArrowFly : MonoBehaviour
{
    [Header("箭矢飛行速度")]
    public float speed = 20f;        // 箭矢往前飛的速度，可以調大一點

    [Header("自動銷毀時間")]
    public float lifeTime = 3f;      // 超過 3 秒沒射到東西就自動刪除，防止場景卡頓

    void Start()
    {
        // 3秒後自動毀滅
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 🌟 關鍵修正：讓箭矢順著「它自己的正前方 (Z軸)」全速前進
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    // 順便幫你寫好撞到敵人的基本逻辑
    void OnTriggerEnter(Collider other)
    {
        // 如果撞到敵人 (假設你的敵人身上掛有 Enemy 腳本)
        if (other.CompareTag("Enemy") || other.GetComponent<Enemy>() != null)
        {
            // 讓敵人扣血（這裡可以串你的傷害邏輯）
            // other.GetComponent<Enemy>().TakeDamage(10); 

            // 箭矢功成身退，銷毀自己
            Destroy(gameObject);
        }
    }
}