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

    [Header("箭矢外觀")]
    public Color arrowWoodColor = new Color(0.55f, 0.32f, 0.13f, 1f);
    public Color arrowWoodEmissionColor = new Color(0.08f, 0.04f, 0.01f, 1f);
    public Color arrowAttackColor = new Color(1f, 0.12f, 0.08f, 1f);
    public Color arrowSpeedColor = new Color(1f, 0.86f, 0.12f, 1f);
    public Color arrowMultiColor = new Color(0.16f, 0.46f, 1f, 1f);

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

    public enum ArrowStyle { Wood, Attack, Speed, Multi }
    private int attackStyleCount = 0;
    private int speedStyleCount = 0;
    private int multiStyleCount = 0;
    private ArrowStyle lastArrowStyle = ArrowStyle.Wood;
    private int killCount = 0;

    void Start()
    {
        currentHp = maxHp;
        SetupStatusTextLayout();
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

    public void RegisterArrowStyle(ArrowStyle style)
    {
        switch (style)
        {
            case ArrowStyle.Attack:
                attackStyleCount++;
                break;
            case ArrowStyle.Speed:
                speedStyleCount++;
                break;
            case ArrowStyle.Multi:
                multiStyleCount++;
                break;
        }

        if (style != ArrowStyle.Wood)
        {
            lastArrowStyle = style;
        }
    }

    public ArrowStyle GetDominantArrowStyle()
    {
        int maxCount = Mathf.Max(attackStyleCount, speedStyleCount, multiStyleCount);
        if (maxCount <= 0) return ArrowStyle.Wood;

        if (lastArrowStyle == ArrowStyle.Attack && attackStyleCount == maxCount) return ArrowStyle.Attack;
        if (lastArrowStyle == ArrowStyle.Speed && speedStyleCount == maxCount) return ArrowStyle.Speed;
        if (lastArrowStyle == ArrowStyle.Multi && multiStyleCount == maxCount) return ArrowStyle.Multi;

        if (attackStyleCount == maxCount) return ArrowStyle.Attack;
        if (speedStyleCount == maxCount) return ArrowStyle.Speed;
        return ArrowStyle.Multi;
    }

    public int GetArrowStyleCount(ArrowStyle style)
    {
        switch (style)
        {
            case ArrowStyle.Attack: return attackStyleCount;
            case ArrowStyle.Speed: return speedStyleCount;
            case ArrowStyle.Multi: return multiStyleCount;
            default: return 0;
        }
    }

    public Color GetArrowColor()
    {
        switch (GetDominantArrowStyle())
        {
            case ArrowStyle.Attack: return arrowAttackColor;
            case ArrowStyle.Speed: return arrowSpeedColor;
            case ArrowStyle.Multi: return arrowMultiColor;
            default: return arrowWoodColor;
        }
    }

    public Color GetArrowEmissionColor()
    {
        switch (GetDominantArrowStyle())
        {
            case ArrowStyle.Attack: return arrowAttackColor;
            case ArrowStyle.Speed: return arrowSpeedColor;
            case ArrowStyle.Multi: return arrowMultiColor;
            default: return arrowWoodEmissionColor;
        }
    }

    private void SetupStatusTextLayout()
    {
        SetupTopLeftText(distanceText, new Vector2(20f, -18f), new Vector2(300f, 42f));
        SetupTopLeftText(killText, new Vector2(20f, -58f), new Vector2(240f, 42f));
    }

    private void SetupTopLeftText(TextMeshProUGUI text, Vector2 position, Vector2 size)
    {
        if (text == null) return;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        text.alignment = TextAlignmentOptions.Left;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
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
