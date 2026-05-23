using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("怪物屬性")]
    public float maxHp = 50f;
    private float currentHp;

    [Header("UI 顯示")]
    public TextMeshPro hpText;

    void Start()
    {
        // 1. 新手保護機制 (Z < 30 不生怪)
        if (transform.position.z < 30f)
        {
            Destroy(gameObject);
            return;
        }

        float scalingDistance = transform.position.z - 30f; 
        if (scalingDistance < 0) scalingDistance = 0; // 防呆，確保距離不會是負數

        float stage = Mathf.Floor(scalingDistance / 40f);

        // 階段 0 的時候，倍率就是乾乾淨淨的 1f！
        float scaleFactor = Mathf.Pow(1.2f, stage);

        maxHp = Mathf.Round(maxHp * scaleFactor);

        currentHp = maxHp;
        UpdateHPUI();

        // 左右與前後隨機座標 (保持不變)
        float randomX = (Random.Range(0, 2) == 0) ? -2.5f : 2.5f;
        float randomZ = transform.localPosition.z + Random.Range(-5f, 5f);
        transform.localPosition = new Vector3(randomX, transform.localPosition.y, randomZ);
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        UpdateHPUI();

        if (currentHp <= 0)
        {
            PlayerStats player = FindAnyObjectByType<PlayerStats>();
            if (player != null)
            {
                player.AddKill();
            }
            Destroy(gameObject);
        }
    }

    void UpdateHPUI()
    {
        if (hpText == null) return;
        if (currentHp < 0) currentHp = 0;
        hpText.text = Mathf.CeilToInt(currentHp).ToString();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerStats player = other.GetComponent<PlayerStats>();

        if (player != null)
        {
            // ==========================================================
            // 🌟 修正 Bug：將扣血機制與怪物當前血量連動！
            // 因為玩家的 TakeDamage 需要整數 (int)，我們用 Mathf.RoundToInt 把怪物的 float 血量四捨五入轉成整數
            // ==========================================================
            int damageToPlayer = Mathf.RoundToInt(currentHp);

            player.TakeDamage(damageToPlayer); // 讓玩家扣除等同於怪物血量的傷害

            Destroy(gameObject); // 怪物自爆消失
        }
    }
}