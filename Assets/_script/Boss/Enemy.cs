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

    private bool isDead = false;
    public bool isClone = false;

    void Start()
    {
        if (transform.position.z < 30f)
        {
            Destroy(gameObject);
            return;
        }

        PlayerStats player = FindAnyObjectByType<PlayerStats>();

        // 🌟 改成固定數量加一的迴圈邏輯
        if (!isClone && player != null && player.extraEnemies > 0)
        {
            for (int i = 0; i < player.extraEnemies; i++)
            {
                Vector3 spawnOffset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
                GameObject clone = Instantiate(gameObject, transform.position + spawnOffset, Quaternion.identity);
                clone.GetComponent<Enemy>().isClone = true;
            }
        }

        float scalingDistance = Mathf.Max(0, transform.position.z - 30f);
        float stage = Mathf.Floor(scalingDistance / 40f);
        maxHp = Mathf.Round(maxHp * Mathf.Pow(1.1f, stage));
        currentHp = maxHp;
        UpdateHPUI();

        if (!isClone)
        {
            float randomX = (Random.Range(0, 2) == 0) ? -2.5f : 2.5f;
            float randomZ = transform.localPosition.z + Random.Range(-5f, 5f);
            transform.localPosition = new Vector3(randomX, transform.localPosition.y, randomZ);
        }
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        FloatingTextSpawner.instance?.Spawn("-" + Mathf.RoundToInt(damage), transform.position, Color.red);
        currentHp -= damage;
        UpdateHPUI();

        if (currentHp <= 0)
        {
            isDead = true;
            PlayerStats player = FindAnyObjectByType<PlayerStats>();
            if (player != null) player.AddKill();

            if (chestPrefab != null)
            {
                Vector3 dropPos = new Vector3(transform.position.x - 0.75f, -0.45f, transform.position.z);
                Instantiate(chestPrefab, dropPos, Quaternion.Euler(0, 180f, 0));
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
        if (isDead) return;
        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            isDead = true;
            player.TakeDamage(Mathf.RoundToInt(currentHp));
            Destroy(gameObject);
        }
    }
}