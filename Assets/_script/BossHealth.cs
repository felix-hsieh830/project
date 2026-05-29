using UnityEngine;
using TMPro;

public class BossHealth : MonoBehaviour

{
    [Header("Boss 數值設定")]
    public float hp = 1000f;
    public float currentHp;
    public bool isBigBoss = false;

    [Header("UI 顯示")]
    public TextMeshPro hpText; // 🌟 用來拉文字組件的格子

    private bool isDead = false;

    // 當 GameManager 設定血量時呼叫
    public void SetupHealth(float newHP)
    {
        hp = newHP;
        currentHp = hp;
        UpdateHPUI(); // 🌟 設定好血量立刻更新文字
    }



    public void TakeDamage(float damage)
    {
        hp -= damage;
        UpdateHPUI(); // 🌟 每次扣血都要更新文字

        if (hp <= 0)
        {
            Die();
        }
    }

    // 🌟 新增：更新頭上文字的功能
    void UpdateHPUI()
    {
        if (hpText == null) return;
        
        if (hp < 0) hp = 0;
        
        // 將血量四捨五入顯示為整數
        hpText.text = Mathf.CeilToInt(hp).ToString();

        // 💡 進階小特效：如果是大 Boss，可以把文字變紅色
        if (isBigBoss) hpText.color = Color.red;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player != null)
        {
            isDead = true; 

            // 1. 玩家扣血
            int damageToPlayer = Mathf.RoundToInt(currentHp); // 這裡你原本寫的是 hp 還是 currentHp 都可以
            player.TakeDamage(damageToPlayer);

            if (player.currentHp > 0) 
            {
                // 活下來了！發放獎勵！
                GameManager gm = FindAnyObjectByType<GameManager>();
                if (gm != null)
                {
                    gm.ShowReward(isBigBoss);
                }
            }

            // 3. 雙方同歸於盡 (Boss 銷毀)
            Destroy(gameObject);
        }
    }

    private void Die()
    {
        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null) gm.ShowReward(isBigBoss);
        Destroy(gameObject);
    }
}
