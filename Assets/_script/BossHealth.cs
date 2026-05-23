using UnityEngine;
using TMPro; // 🌟 記得加這行才能使用文字組件

public class BossHealth : MonoBehaviour
{
    [Header("Boss 數值設定")]
    public float hp = 1000f;
    public bool isBigBoss = false;

    [Header("UI 顯示")]
    public TextMeshPro hpText; // 🌟 用來拉文字組件的格子

    // 當 GameManager 設定血量時呼叫
    public void SetupHealth(float newHP)
    {
        hp = newHP;
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

    private void Die()
    {
        GameManager gm = FindAnyObjectByType<GameManager>();
        if (gm != null) gm.ShowReward(isBigBoss);
        Destroy(gameObject);
    }
}