using UnityEngine;
using TMPro;

public class BuffGate : MonoBehaviour
{
    public enum BuffType { AttackSpeed, AttackRange, CritRate, Damage, MaxHP }

    [Header("UI 顯示")]
    public TextMeshPro buffText;

    [Header("抽籤結果")]
    public BuffType myBuffType;
    public float buffValue;
    public string rarity;

    void Start()
    {
        RollBuffType();
        RollBuffValue();
        UpdateUI();

        // 🌟 抽完籤、寫好字之後，立刻幫大門塗上對應的顏色！
        UpdateGateColor();
    }

    // 🌟 新增這個變色魔法區塊
    void UpdateGateColor()
    {
        // 抓取門身上的 MeshRenderer (負責顯示外觀的元件)
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return; // 防呆機制

        // 設定半透明度 (0.0 到 1.0，0.5 代表 50% 半透明)
        // 這樣才不會把你剛剛辛苦設定的玻璃質感蓋掉！
        float alpha = 0.5f;

        // 根據稀有度換顏色 (Unity 裡的顏色數值是 0f ~ 1f)
        if (rarity == "普通")
        {
            // 白色半透明
            renderer.material.color = new Color(1f, 1f, 1f, alpha);
        }
        else if (rarity == "稀有")
        {
            // 藍色半透明
            renderer.material.color = new Color(0.2f, 0.5f, 1f, alpha);
        }
        else if (rarity == "傳說")
        {
            // 紫色半透明 (紅 + 藍 = 紫)
            renderer.material.color = new Color(0.7f, 0.2f, 0.9f, alpha);
        }
    }

    void UpdateUI()
    {
        if (buffText == null) return;

        string buffNameInChinese = "";
        switch (myBuffType)
        {
            case BuffType.AttackSpeed: buffNameInChinese = "攻擊速度"; break;
            case BuffType.AttackRange: buffNameInChinese = "攻擊距離"; break;
            case BuffType.CritRate: buffNameInChinese = "爆擊機率"; break;
            case BuffType.Damage: buffNameInChinese = "攻擊力"; break;
            case BuffType.MaxHP: buffNameInChinese = "最大生命"; break;
        }

        buffText.text = $"{buffNameInChinese}\n+{buffValue}";
    }

    void RollBuffType()
    {
        int roll = Random.Range(0, 100);
        if (roll < 20) myBuffType = BuffType.AttackSpeed;
        else if (roll < 40) myBuffType = BuffType.CritRate;
        else if (roll < 60) myBuffType = BuffType.AttackRange;
        else if (roll < 80) myBuffType = BuffType.MaxHP;
        else myBuffType = BuffType.Damage;
    }

    void RollBuffValue()
    {
        float baseVal = 0;
        switch (myBuffType)
        {
            case BuffType.AttackSpeed: baseVal = 0.2f; break;
            case BuffType.AttackRange: baseVal = 3f; break;
            case BuffType.CritRate: baseVal = 0.05f; break;
            case BuffType.Damage: baseVal = 2f; break;
            case BuffType.MaxHP: baseVal = 20f; break;
        }

        // 抽稀有度倍率 (你的 60 / 30 / 10 設定)
        int rarityRoll = Random.Range(0, 100);
        float rarityMultiplier = 1f;

        if (rarityRoll < 60) { rarityMultiplier = 1f; rarity = "普通"; }
        else if (rarityRoll < 90) { rarityMultiplier = 2f; rarity = "稀有"; }
        else { rarityMultiplier = 4f; rarity = "傳說"; }
        float scalingDistance = transform.position.z - 30f;
        if (scalingDistance < 0) scalingDistance = 0;

        // 假設門的階段比較長，每 100 公尺才算 1 個階段
        float stage = Mathf.Floor(scalingDistance / 100f);

        // 🌟 大門的指數成長 (因為門給的是玩家能力，倍率設小一點，例如 1.1 的 stage 次方)
        float distanceMultiplier = Mathf.Pow(1.1f, stage);

        // 算出最終數值
        buffValue = baseVal * rarityMultiplier * distanceMultiplier;
        if (myBuffType == BuffType.CritRate || myBuffType == BuffType.AttackSpeed)
        {
            buffValue = (float)System.Math.Round(buffValue, 2);
        }
        else
        {
  
            buffValue = Mathf.Round(buffValue);
        }
        if (myBuffType == BuffType.MaxHP)
        {
            buffValue = Mathf.Round(buffValue);
        }
        else
        {
            buffValue = (float)System.Math.Round(buffValue, 1);
        }
    }

    private bool hasTriggered = false;
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null)
        {
            // 🌟 碰到的瞬間立刻上鎖，就算同一個畫面撞到兩次也進不來了！
            hasTriggered = true;

            // 1. 玩家獲得能力加成
            switch (myBuffType)
            {
                case BuffType.AttackSpeed: stats.attackSpeed += buffValue; break;
                case BuffType.AttackRange: stats.attackRange += buffValue; break;
                case BuffType.CritRate: stats.critRate += buffValue; break;
                case BuffType.Damage: stats.baseDamage += buffValue; break;
                case BuffType.MaxHP: stats.AddMaxHealth((int)buffValue); break;
            }

            GameManager gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.AddDoorCount();
            }


            if (transform.parent != null) Destroy(transform.parent.gameObject);
            else Destroy(gameObject);
        }
    }
}