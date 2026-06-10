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
        UpdateGateColor();
    }

    void UpdateGateColor()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return;

        float alpha = 0.5f;
        if (rarity == "普通") renderer.material.color = new Color(1f, 1f, 1f, alpha);
        else if (rarity == "稀有") renderer.material.color = new Color(0.2f, 0.5f, 1f, alpha);
        else if (rarity == "傳說") renderer.material.color = new Color(0.7f, 0.2f, 0.9f, alpha);
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

        if (myBuffType == BuffType.CritRate)
        {
            buffText.text = $"{buffNameInChinese}\n+{FormatPercent(buffValue)}";
        }
        else
        {
            buffText.text = $"{buffNameInChinese}\n+{buffValue}";
        }
    }

    void RollBuffType()
    {
        int roll = Random.Range(0, 100);
        if (roll < 24) myBuffType = BuffType.Damage;
        else if (roll < 48) myBuffType = BuffType.AttackSpeed;
        else if (roll < 66) myBuffType = BuffType.AttackRange;
        else if (roll < 82) myBuffType = BuffType.MaxHP;
        else myBuffType = BuffType.CritRate;
    }

    string FormatPercent(float value)
    {
        return (value * 100f).ToString("0.#") + "%";
    }

    void RollBuffValue()
    {
        float baseVal = 0;
        switch (myBuffType)
        {
            case BuffType.AttackSpeed: baseVal = 0.18f; break;
            case BuffType.AttackRange: baseVal = 1.0f; break;
            case BuffType.CritRate: baseVal = 0.01f; break;
            case BuffType.Damage: baseVal = 2.5f; break;
            case BuffType.MaxHP: baseVal = 20f; break;
        }

        int rarityRoll = Random.Range(0, 100);
        float rarityMultiplier = 1f;

        if (rarityRoll < 72) { rarityMultiplier = 1f; rarity = "普通"; }
        else if (rarityRoll < 95) { rarityMultiplier = 1.35f; rarity = "稀有"; }
        else { rarityMultiplier = 1.8f; rarity = "傳說"; }

        float scalingDistance = transform.position.z - 30f;
        if (scalingDistance < 0) scalingDistance = 0;

        float stage = Mathf.Floor(scalingDistance / 80f);
        float distanceMultiplier = 1f + stage * 0.06f;

        buffValue = baseVal * rarityMultiplier * distanceMultiplier;

        if (myBuffType == BuffType.AttackRange)
        {
            buffValue = rarity == "普通" ? 1f : 2f;
        }

        if (myBuffType == BuffType.MaxHP)
            buffValue = Mathf.Round(buffValue);
        else if (myBuffType == BuffType.CritRate)
            buffValue = (float)System.Math.Round(buffValue, 3);
        else if (myBuffType == BuffType.AttackSpeed)
            buffValue = (float)System.Math.Round(buffValue, 2);
        else
            buffValue = (float)System.Math.Round(buffValue, 1);
    }

    private bool hasTriggered = false;
    void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        PlayerStats stats = other.GetComponent<PlayerStats>();
        if (stats != null)
        {
            hasTriggered = true;

            string floatingMsg = buffText.text.Replace("\n", " ");
            FloatingTextSpawner.instance?.Spawn(floatingMsg, other.transform.position, Color.green, Vector3.up, other.transform, 0.65f, 2.35f);

            switch (myBuffType)
            {
                case BuffType.AttackSpeed: stats.attackSpeed += buffValue; break;
                case BuffType.AttackRange: stats.attackRange += buffValue; break;
                case BuffType.CritRate: stats.critRate += buffValue; break;
                case BuffType.Damage: stats.baseDamage += buffValue; break;
                case BuffType.MaxHP: stats.AddMaxHealth((int)buffValue); break;
            }

            if (transform.parent != null) Destroy(transform.parent.gameObject);
            else Destroy(gameObject);
        }
    }
}
