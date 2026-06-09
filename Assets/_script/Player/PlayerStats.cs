using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("生存屬性")]
    public int maxHp = 200;         // 🌟 100 → 200
    public int currentHp = 200;

    [Header("攻擊屬性")]
    public float baseDamage = 15f;  // 🌟 10 → 15
    public float attackSpeed = 1f;
    public float attackRange = 10f; // 🌟 20 → 10，讓成長有感
    public int arrowCount = 1;

    [Header("爆擊屬性")]
    public float critRate = 0.1f;
    public float critDamage = 2.0f;

    [Header("🌟 特殊天賦等級 (Boss獎勵)")]
    public int lifestealLevel = 0;
    public int collisionResistLevel = 0;
    public int extraEnemies = 0;
    public int magnetLevel = 0;

    [Header("UI 顯示")]
    public TextMeshPro hpText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI killText;
    public GameManager gameManager;

    private int killCount = 0;

    void Start()
    {
        currentHp = maxHp;
        UpdateHPUI();
        if (killText != null) killText.text = "擊殺: 0";
    }

    void Update()
    {
        int distance = Mathf.FloorToInt(transform.position.z) - 30;
        if (distance < 0) distance = 0;

        if (distanceText != null)
            distanceText.text = "距離: " + FormatNumber(distance) + " m";
    }

    public void AddKill()
    {
        killCount++;
        if (killText != null) killText.text = "擊殺: " + FormatNumber(killCount);
    }

    public void Heal(int amount, bool showFloatingTextWhenFull = false)
    {
        if (currentHp <= 0) return;

        int hpBeforeHeal = currentHp;
        currentHp = currentHp + amount; // 🌟 允許超過最大生命（吸血用）
        int actualHeal = currentHp - hpBeforeHeal;

        UpdateHPUI();
        if (actualHeal > 0 || showFloatingTextWhenFull)
        {
            FloatingTextSpawner.instance?.Spawn("+" + actualHeal.ToString(), transform.position, Color.green, Vector3.right, transform, 1.15f);
        }
    }

    public void TakeDamage(int damage)
    {
        float damageReduction = 1f - (collisionResistLevel * 0.1f);
        int finalDamage = Mathf.RoundToInt(damage * damageReduction);

        FloatingTextSpawner.instance?.Spawn("-" + finalDamage.ToString(), transform.position, Color.red, Vector3.right, transform, 1.25f);
        currentHp -= finalDamage;
        UpdateHPUI();

        if (currentHp <= 0)
        {
            int finalDistance = Mathf.FloorToInt(transform.position.z) - 30;
            if (finalDistance < 0) finalDistance = 0;
            if (gameManager != null) gameManager.ShowGameOver(finalDistance, killCount);

            MonoBehaviour movementScript = GetComponent("CubeMovement") as MonoBehaviour;
            if (movementScript != null) movementScript.enabled = false;
            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void AddMaxHealth(int amount)
    {
        maxHp += amount;
        currentHp += amount;
        UpdateHPUI();
    }

    void UpdateHPUI()
    {
        if (hpText == null) return;
        if (currentHp < 0) currentHp = 0;
        hpText.text = FormatNumber(currentHp);
        hpText.ForceMeshUpdate(true, true);
        Canvas.ForceUpdateCanvases();
    }

    public string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.##") + "B";
        else if (number >= 1000000) return (number / 1000000f).ToString("0.##") + "M";
        else if (number >= 1000) return (number / 1000f).ToString("0.##") + "K";
        return Mathf.FloorToInt(number).ToString();
    }
}
