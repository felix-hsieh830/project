using UnityEngine;

public class ArrowFly : MonoBehaviour
{
    public float speed = 15f; // 這是箭矢的基本發射力道
    private float totalSpeed; //  新增：最終疊加出來的絕對速度
    private float finalDamage;

    // 🌟 修改：多接收第 5 個參數 inheritedSpeed (玩家當前的跑速)
    public void Setup(float playerDamage, float playerRange, float critRate, float critDamage, float inheritedSpeed)
    {
        // 1. 決定有沒有爆擊
        if (Random.value <= critRate)
        {
            // 爆擊啦！傷害翻倍，而且把箭變大 1.5 倍看起來比較爽！
            finalDamage = playerDamage * critDamage;
            transform.localScale *= 1.5f;
        }
        else
        {
            // 沒爆擊，一般傷害
            finalDamage = playerDamage;
        }

        // ==========================================
        //  2. 物理動能疊加：最終速度 = 箭的初速 + 玩家當前跑速
        // ==========================================
        totalSpeed = speed + inheritedSpeed;

        //  3. 修正壽命計算：必須用「疊加後的速度 (totalSpeed)」來算壽命
        // 這樣不管箭飛多快，它的最終射程 (playerRange) 都會是精準的！
        float lifeTime = playerRange / totalSpeed;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 修改：移動時使用 totalSpeed 替代原本的 speed
        transform.Translate(0, 0, totalSpeed * Time.deltaTime);
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