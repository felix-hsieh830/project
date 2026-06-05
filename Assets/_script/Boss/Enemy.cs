using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    [Header("怪物屬性")]
    public float maxHp = 30f;
    private float currentHp;

    [Header("UI 顯示")]
    public TextMeshPro hpText;

    [Header("掉落設定")]
    public GameObject chestPrefab;

    private bool isDead = false; // 防止同幀多次觸發死亡

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
        float scaleFactor = Mathf.Pow(1.1f, stage);

        maxHp = Mathf.Round(maxHp * scaleFactor);

        currentHp = maxHp;
        UpdateHPUI();

        // 左右與前後隨機座標 (保持不變)
        float randomX = (Random.Range(0, 2) == 0) ? -2.5f : 2.5f;
        float randomZ = transform.localPosition.z + Random.Range(-5f, 5f);
        transform.localPosition = new Vector3(randomX, transform.localPosition.y, randomZ);
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return; // 已死亡，擋掉重複呼叫

        FloatingTextSpawner.instance?.Spawn("-"+damage.ToString(), transform.position, Color.red);
        currentHp -= damage;
        UpdateHPUI();

        if (currentHp <= 0)
        {
            isDead = true; // 立刻鎖上，防止同幀第二次進來

            PlayerStats player = FindAnyObjectByType<PlayerStats>();
            if (player != null)
            {
                player.AddKill();
            }

            if (chestPrefab != null)
            {
                Vector3 dropPos = new Vector3(transform.position.x-0.75f, -0.45f, transform.position.z);
                Quaternion chestRotation = Quaternion.Euler(0, 180f, 0);
                Instantiate(chestPrefab, dropPos, chestRotation);
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
        if (isDead) return; // 同樣的保護

        PlayerStats player = other.GetComponent<PlayerStats>();

        if (player != null)
        {
            isDead = true; // 鎖上

            int damageToPlayer = Mathf.RoundToInt(currentHp);
            player.TakeDamage(damageToPlayer);

            Destroy(gameObject);
        }
    }
}