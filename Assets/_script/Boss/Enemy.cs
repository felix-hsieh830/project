using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("怪物屬性")]
    public float maxHp = 30f;
    private float currentHp;
    public float CurrentHp => currentHp;

    [Header("UI 顯示")]
    public TextMeshPro hpText;

    [Header("掉落設定")]
    public GameObject chestPrefab;

    private bool isDead = false;
    public bool isClone = false;
    private int spawnedExtraEnemies = 0;
    private bool hasInitializedHealth = false;
    private bool skipInitialHpScaling = false;

    // 🌟 靜態列表取代 FindObjectsByType，提升效能
    private static List<Enemy> allEnemies = new List<Enemy>();

    void OnEnable() { allEnemies.Add(this); }
    void OnDisable() { allEnemies.Remove(this); }

    void Start()
    {
        if (transform.position.z < 30f)
        {
            Destroy(gameObject);
            return;
        }

        PlayerStats player = FindAnyObjectByType<PlayerStats>();

        if (!isClone)
        {
            float randomX = (Random.Range(0, 2) == 0) ? -2.5f : 2.5f;
            float randomZ = transform.localPosition.z + Random.Range(-5f, 5f);
            transform.localPosition = new Vector3(randomX, transform.localPosition.y, randomZ);

            if (player != null) EnsureExtraEnemyCount(player.extraEnemies);
        }

        if (!skipInitialHpScaling)
        {
            float scalingDistance = Mathf.Max(0, transform.position.z - 30f);
            float stage = Mathf.Floor(scalingDistance / 80f);
            maxHp = Mathf.Round(maxHp * (1.45f + stage * 0.28f));
        }

        currentHp = maxHp;
        hasInitializedHealth = true;
        UpdateHPUI();
    }

    public void EnsureExtraEnemyCount(int desiredCount)
    {
        if (isClone || desiredCount <= spawnedExtraEnemies) return;

        int countToSpawn = desiredCount - spawnedExtraEnemies;
        SpawnExtraEnemies(countToSpawn);
        spawnedExtraEnemies = desiredCount;
    }

    public static void RefreshAllExtraEnemies(int desiredCount)
    {
        Enemy[] enemies = allEnemies.ToArray();
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null) enemy.EnsureExtraEnemyCount(desiredCount);
        }
    }

    public static void ClearAllExtraEnemies()
    {
        Enemy[] enemies = allEnemies.ToArray();
        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;
            if (enemy.isClone) Destroy(enemy.gameObject);
            else enemy.spawnedExtraEnemies = 0;
        }
    }

    private void SpawnExtraEnemies(int count)
    {
        Transform spawnParent = transform.parent;
        Vector3 baseLocalPos = transform.localPosition;

        for (int i = 0; i < count; i++)
        {
            int slotIndex = spawnedExtraEnemies + i;
            float laneX = (slotIndex % 2 == 0) ? -baseLocalPos.x : baseLocalPos.x;
            if (Mathf.Abs(laneX) < 1f) laneX = (slotIndex % 2 == 0) ? -2.5f : 2.5f;

            float zOffset = 6f + (slotIndex / 2) * 4f + Random.Range(-1f, 1f);
            if (slotIndex % 2 == 1) zOffset *= -1f;

            Vector3 cloneLocalPos = new Vector3(laneX, baseLocalPos.y, baseLocalPos.z + zOffset);
            cloneLocalPos.x = Mathf.Clamp(cloneLocalPos.x, -3.5f, 3.5f);

            Vector3 cloneWorldPos = spawnParent != null ? spawnParent.TransformPoint(cloneLocalPos) : cloneLocalPos;
            GameObject clone = Instantiate(gameObject, cloneWorldPos, transform.rotation, spawnParent);

            Enemy cloneEnemy = clone.GetComponent<Enemy>();
            if (cloneEnemy != null)
            {
                cloneEnemy.isClone = true;
                cloneEnemy.skipInitialHpScaling = hasInitializedHealth;
                if (hasInitializedHealth)
                {
                    cloneEnemy.maxHp = maxHp;
                    cloneEnemy.currentHp = maxHp;
                }
                cloneEnemy.transform.localPosition = cloneLocalPos;
                if (hasInitializedHealth) cloneEnemy.UpdateHPUI();
            }
        }
    }

    public virtual bool TakeDamage(float damage)
    {
        if (isDead) return false;

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

        return true;
    }

    void UpdateHPUI()
    {
        if (hpText == null) return;
        if (currentHp < 0) currentHp = 0;
        hpText.text = FormatNumber(currentHp);
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

    public string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.#") + "B";
        else if (number >= 1000000) return (number / 1000000f).ToString("0.#") + "M";
        else if (number >= 1000) return (number / 1000f).ToString("0.#") + "K";
        return Mathf.FloorToInt(number).ToString();
    }
}
