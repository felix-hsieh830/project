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
    void Awake()
    {
        instance = this;
    }
    public void Spawn(string text, Vector3 position, Color color)
    {
        // 1. 計算隨機的偏移量
        Vector3 randomPos = new Vector3(
            Random.Range(-randomOffset, randomOffset),
            heightOffset,
            Random.Range(-randomOffset, randomOffset)
        );
        // 2. 最終生成位置 = 傳進來的原始位置 (通常是怪物中心或腳底) + 我們設定的偏移量
        Vector3 spawnPosition = position + randomPos;
        // 3. 生成數字
        GameObject obj = Instantiate(floatingTextPrefab, spawnPosition, Quaternion.identity);
        TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.color = color;
        }
    }
}