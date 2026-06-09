using UnityEngine;
using TMPro;

public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner instance;
    public GameObject floatingTextPrefab;

    [Header("生成位置微調")]
    [Tooltip("往上偏移的高度，請依據你的怪物身高調整")]
    public float heightOffset = 1.0f;
    [Tooltip("左右前後隨機散開的範圍，避免數字全疊在一起")]
    public float randomOffset = 0.5f;
    public float defaultFloatSpeed = 2f;
    public float rewardFloatSpeed = 1f;
    public float rewardHeightOffset = 6f;
    public float rightSideOffset = 1.35f;

    void Awake()
    {
        instance = this;
    }

    // 🌟 direction: 飄移方向 (預設往上)
    // 🌟 parent: 跟著誰走 (傳入角色 transform 就會跟著角色，不填則留在原地)
    public void Spawn(string text, Vector3 position, Color color, Vector3 direction = default, Transform parent = null)
    {
        Spawn(text, position, color, direction, parent, defaultFloatSpeed);
    }

    public void Spawn(string text, Vector3 position, Color color, Vector3 direction, Transform parent, float floatSpeed)
    {
        Spawn(text, position, color, direction, parent, floatSpeed, heightOffset);
    }

    public void Spawn(string text, Vector3 position, Color color, Vector3 direction, Transform parent, float floatSpeed, float spawnHeightOffset)
    {
        if (direction == default)
            direction = Vector3.up;

        Vector3 offset;
        Vector3 floatDirection = direction;

        if (direction == Vector3.right)
        {
            offset = new Vector3(rightSideOffset, spawnHeightOffset, 0f);
            floatDirection = Vector3.up;
        }
        else
        {
            offset = new Vector3(0f, spawnHeightOffset, 0f);
        }

        Vector3 spawnPosition = position + offset;
        GameObject obj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);

        // 🌟 綁在父物件上，飄字就會跟著移動
        if (parent != null)
            obj.transform.SetParent(parent);

        TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }

        FloatingText ft = obj.GetComponent<FloatingText>();
        if (ft != null)
        {
            ft.floatDirection = floatDirection;
            ft.floatSpeed = floatSpeed;
        }
        else
        {
            Debug.Log("找不到 FloatingText 組件！");
        }                                   
    }
}
    
