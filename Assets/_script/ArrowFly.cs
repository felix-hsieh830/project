using UnityEngine;

public class ArrowFly : MonoBehaviour
{
    public float speed = 15f;
    private float finalDamage; // 最終算出來的傷害

    // 這是一個「接收設定」的魔法，當玩家射箭時會呼叫它
    public void Setup(float playerDamage, float playerRange, float critRate, float critDamage)
    {
        // 1. 決定有沒有爆擊 (Random.value 會骰出 0.0 ~ 1.0 的數字)
        if (Random.value <= critRate)
        {
            // 爆擊啦！傷害翻倍，而且把箭變大 1.5 倍看起來比較爽！
            finalDamage = playerDamage * critDamage;
            transform.localScale *= 1.5f;
            // 如果你有紅色材質球，甚至可以在這裡把箭變成紅色的！
        }
        else
        {
            // 沒爆擊，一般傷害
            finalDamage = playerDamage;
        }

        // 2. 算壽命：壽命(秒) = 距離 / 速度。時間到了箭就會自動銷毀
        float lifeTime = playerRange / speed;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);
    }
    void OnTriggerEnter(Collider other)
        {
            // 檢查撞到的東西身上，有沒有掛著 Enemy 腳本？
            Enemy target = other.GetComponent<Enemy>();

            if (target != null)
            {
                // 如果有，代表我們射中怪物了！
                // 呼叫怪物的扣血函數，並把這支箭的 finalDamage 傳給它
                target.TakeDamage(finalDamage);

                // 箭矢完成了它的使命，把自己銷毀
                Destroy(gameObject);
        }
    }
}