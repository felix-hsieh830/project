using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    [Header("生存屬性")]
    public int maxHp = 100;
    public int currentHp = 100;

    [Header("攻擊屬性")]
    public float baseDamage = 10f;
    public float attackSpeed = 1f;
    public float attackRange = 20f;
    public int arrowCount = 1;

    [Header("爆擊屬性")]
    public float critRate = 0.1f;
    public float critDamage = 2.0f;

    [Header("UI 顯示")]
    public TextMeshPro hpText; // 🌟 用來裝主角頭頂文字的格子
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI killText;
    public GameManager gameManager;

    // 🌟 新增：紀錄殺怪數量
    private int killCount = 0;

    void Start()
    {
        currentHp = maxHp;
        UpdateHPUI(); // 🌟 遊戲開始時立刻顯示血量
        if (killText != null) killText.text = "擊殺: 0";
    }

    void Update()
    {
        // 把主角當前的 Z 軸座標當作距離 (無條件捨去小數點)
        // 扣掉 30 是因為我們把前 30 公尺當作不計分的新手村！
        int distance = Mathf.FloorToInt(transform.position.z) - 30;
        if (distance < 0) distance = 0;

        // 🌟 修改：用 FormatNumber 包裝距離數字！
        if (distanceText != null)
        {
            distanceText.text = "距離: " + FormatNumber(distance) + " m";
        }
    }


    // 🌟 新增：專門給怪物呼叫的加分魔法
    public void AddKill()
    {
        killCount++;
        // 🌟 修改：用 FormatNumber 包裝擊殺數字！
        if (killText != null)
        {
            killText.text = "擊殺: " + FormatNumber(killCount);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        UpdateHPUI();

        if (currentHp <= 0)
        {
            // 🌟 死亡結算：算出當下距離
            int finalDistance = Mathf.FloorToInt(transform.position.z) - 30;
            if (finalDistance < 0) finalDistance = 0;

            // 🌟 呼叫總管顯示面板，並傳入最終距離與擊殺數！
            if (gameManager != null)
            {
                gameManager.ShowGameOver(finalDistance, killCount);
            }
            MonoBehaviour movementScript = GetComponent("CubeMovement") as MonoBehaviour;
            if (movementScript != null)
            {
                movementScript.enabled = false; // 禁用移動腳本
                Debug.Log("死掉啦！移動腳本已關閉！");
            }

            GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void AddMaxHealth(int amount)
    {
        maxHp += amount;
        currentHp += amount;

        // 🌟 最關鍵的一行：數字改完之後，一定要呼叫這個魔法刷新 3D 文字！
        UpdateHPUI();

        Debug.Log("吃到血包了！最大生命增加 " + amount + "。目前血量：" + currentHp);
    }

    // 🌟 新增：純數字血量更新魔法
    void UpdateHPUI()
    {
        if (hpText == null) return; // 防呆
        if (currentHp < 0) currentHp = 0;

        // 🌟 修改：用 FormatNumber 包裝血量數字，變成超酷的 1K 顯示法！
        hpText.text = FormatNumber(currentHp);
    }

    // ==========================================
    // 🌟 數字縮寫轉換機 (將大數字轉成 K, M, B)
    // 直接複製一份過來給 UI 即時更新使用！
    // ==========================================
    public string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.##") + "B";
        else if (number >= 1000000) return (number / 1000000f).ToString("0.##") + "M";
        else if (number >= 1000) return (number / 1000f).ToString("0.##") + "K";

        return Mathf.FloorToInt(number).ToString();
    }
}