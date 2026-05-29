using UnityEngine;

public class ArrowFly : MonoBehaviour
{
    public float speed = 15f; // 這是箭矢的基本發射力道
    private float totalSpeed; //  新增：最終疊加出來的絕對速度
    private float finalDamage;

    // 🌟 修改：多接收第 5 個參數 inheritedSpeed (玩家當前的跑速)
    // 🌟 新增第六個參數：flightSpeedMultiplier
    public void Setup(float playerDamage, float playerRange, float critRate, float critDamage, float inheritedSpeed, float flightSpeedMultiplier)
    {
        if (Random.value <= critRate)
        {
            finalDamage = playerDamage * critDamage;
            transform.localScale *= 1.8f;
        }
        else
        {
            finalDamage = playerDamage;
        }

        // ==========================================
        // 🌟 核心修改：箭矢基礎速度 x 飛速倍率，最後再疊加玩家跑速
        // ==========================================
        float baseSpeed = speed * flightSpeedMultiplier;
        totalSpeed = baseSpeed + inheritedSpeed;

        float finalRange = Mathf.Min(playerRange, 90f);
        float lifeTime = finalRange / totalSpeed;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 1. 算出這 1/60 秒內，箭矢「預計」要往前衝多遠
        float stepDistance = totalSpeed * Time.deltaTime;

        RaycastHit hit;
        
        // 參數：(起點, 方向, 裝打到的東西, 雷射長度)
        if (Physics.Raycast(transform.position, transform.forward, out hit, stepDistance))
        {
            // 如果雷射在這短短的距離內，掃到東西了！
            Enemy target = hit.collider.GetComponent<Enemy>();
            if (target != null)
            {
                target.TakeDamage(finalDamage);
                Destroy(gameObject);
                return; // 撞到了就提早收工，不要再往前移動了
            }

            BossHealth boss = hit.collider.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(finalDamage);
                Destroy(gameObject);
                return;
            }
        }

        // 3. 確定前方安全，雷射沒掃到怪，才真正往前移動
        transform.Translate(0, 0, stepDistance);
    }

    void OnTriggerEnter(Collider other)
    {
        Enemy target = other.GetComponent<Enemy>();
        if (target != null)
        {
            // 呼叫怪物的扣血函數，並把這支箭的 finalDamage 傳給它
            target.TakeDamage(finalDamage);
            // 箭矢完成了它的使命，把自己銷毀
            Destroy(gameObject);
            return; // 提早結束，這樣才不會發生一支箭同時射穿兩個東西的 Bug
        }

        BossHealth boss = other.GetComponent<BossHealth>();
        if (boss != null)
        {
            // 呼叫 Boss 的扣血函數，一樣把 finalDamage 傳給它！
            boss.TakeDamage(finalDamage);
            // 箭矢完成使命，把自己銷毀
            Destroy(gameObject);
            return;
        }
    }
}