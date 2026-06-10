using System.Collections;
using UnityEngine;

public class BigBossAI : MonoBehaviour
{
    [Header("階段一：移動設定 (滑步)")]
    public float moveSpeed = 4f;
    public float limitX = 4f;
    private int moveDirection = 1;

    [Header("階段一：攻擊設定 (火焰壓制)")]
    public float fireRate = 1.25f;
    public float warningTime = 0.55f;
    public float phaseOneProjectileSpeed = 17f;
    public float phaseOneProjectileLifeTime = 2.05f;
    public int phaseOneProjectileDamage = 35;
    public float laneWidth = 1.45f;
    public float warningLength = 34f;
    private float fireTimer = 0f;
    private int phaseOneAttackIndex = 0;
    private bool isPhaseOneAttacking = false;
    private readonly float[] phaseOneLanes = { -3f, 0f, 3f };

    [Header("階段二：死靈召喚設定")]
    public GameObject minionPrefab;   // 🌟 準備用來召喚的小怪
    public float summonRate = 3f;     // 每隔幾秒召喚一波
    private float summonTimer = 0f;

    private BossHealth healthScript;
    private bool isActive = false;
    private bool isPhaseTwo = false;  // 🌟 紀錄是否進入第二階段

    void Start()
    {
        healthScript = GetComponent<BossHealth>();
    }

    public void ApplyEncounterScaling(int totalBossSpawnCount)
    {
        int bigBossEncounterIndex = Mathf.Max(1, Mathf.CeilToInt(totalBossSpawnCount / 4f));
        float damageMultiplier = 1f + Mathf.Max(0, bigBossEncounterIndex - 1) * 0.32f;
        phaseOneProjectileDamage = Mathf.Max(1, Mathf.RoundToInt(phaseOneProjectileDamage * Mathf.Min(3f, damageMultiplier)));
    }

    void Update()
    {
        // Boss 死了就不動
        if (healthScript != null && healthScript.hp <= 0) return;

        if (healthScript != null && healthScript.isInvincible) return;

        // 偵測玩家是否就位
        if (!isActive)
        {
            PlayerMove player = FindAnyObjectByType<PlayerMove>();
            if (player != null && player.isFightingBigBoss)
            {
                isActive = true;
            }
            return;
        }

        // ==========================================
        // 🌟 核心：判斷是否該進入第二階段 (血量 <= 50%)
        // ==========================================
        // 在你的 BossHealth 裡，currentHp 裝的是最大血量，hp 才是當前血量
        if (!isPhaseTwo && healthScript.currentHp <= healthScript.hp * 0.5f)
        {
            isPhaseTwo = true;
            Debug.Log("💀 大 Boss 進入第二階段：死靈召喚！");

            // 變身時，強制把 Boss 瞬移回正中間，比較有魔王的氣勢
            transform.position = new Vector3(0, transform.position.y, transform.position.z);
        }

        // ==========================================
        // 執行階段行為
        // ==========================================
        if (!isPhaseTwo)
        {
            // 【第一階段】左右滑步 + 射雷射
            transform.Translate(Vector3.right * moveDirection * moveSpeed * Time.deltaTime);

            if (transform.position.x >= limitX)
            {
                moveDirection = -1;
                transform.position = new Vector3(limitX, transform.position.y, transform.position.z);
            }
            else if (transform.position.x <= -limitX)
            {
                moveDirection = 1;
                transform.position = new Vector3(-limitX, transform.position.y, transform.position.z);
            }

            fireTimer += Time.deltaTime;
            if (!isPhaseOneAttacking && fireTimer >= fireRate)
            {
                StartCoroutine(PhaseOneAttackRoutine());
                fireTimer = 0f;
            }
        }
        else
        {
            // 【第二階段】站在原地 + 召喚死靈
            summonTimer += Time.deltaTime;
            if (summonTimer >= summonRate)
            {
                SummonMinions();
                summonTimer = 0f;
            }
        }
    }

    private IEnumerator PhaseOneAttackRoutine()
    {
        isPhaseOneAttacking = true;

        if (phaseOneAttackIndex % 2 == 0)
        {
            yield return StartCoroutine(FireLaneWave());
        }
        else
        {
            yield return StartCoroutine(FireAimedBurst());
        }

        phaseOneAttackIndex++;
        isPhaseOneAttacking = false;
    }

    private IEnumerator FireLaneWave()
    {
        int safeLane = phaseOneAttackIndex % phaseOneLanes.Length;
        for (int i = 0; i < phaseOneLanes.Length; i++)
        {
            if (i == safeLane) continue;
            CreateLaneWarning(phaseOneLanes[i]);
        }

        yield return new WaitForSeconds(warningTime);

        for (int i = 0; i < phaseOneLanes.Length; i++)
        {
            if (i == safeLane) continue;
            SpawnPhaseOneFireball(new Vector3(phaseOneLanes[i], 0.35f, transform.position.z - 1.8f), Vector3.back);
        }
    }

    private IEnumerator FireAimedBurst()
    {
        PlayerStats player = FindAnyObjectByType<PlayerStats>();
        float targetX = player != null ? Mathf.Clamp(player.transform.position.x, -limitX, limitX) : 0f;
        CreateLaneWarning(targetX);

        yield return new WaitForSeconds(warningTime * 0.75f);

        Vector3 baseSpawn = transform.position + new Vector3(0f, 0.45f, -1.8f);
        for (int i = -1; i <= 1; i++)
        {
            Vector3 target = new Vector3(targetX + i * 0.85f, 0.45f, baseSpawn.z - 12f);
            Vector3 direction = target - baseSpawn;
            direction.y = 0f;
            SpawnPhaseOneFireball(baseSpawn + new Vector3(i * 0.35f, 0f, 0f), direction.normalized);
            yield return new WaitForSeconds(0.12f);
        }
    }

    private void CreateLaneWarning(float x)
    {
        GameObject warning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        warning.name = "BigBossFireWarning";
        warning.transform.position = new Vector3(x, 0.04f, transform.position.z - warningLength * 0.5f - 1.5f);
        warning.transform.localScale = new Vector3(laneWidth, 0.06f, warningLength);

        Renderer renderer = warning.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = CreateRuntimeMaterial(new Color(1f, 0.12f, 0.03f, 0.45f));
        }

        Collider collider = warning.GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        Destroy(warning, warningTime + 0.12f);
    }

    private void SpawnPhaseOneFireball(Vector3 spawnPos, Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.01f) direction = Vector3.back;
        SfxManager.Play("boss_fireball", 0.62f, 0.12f);

        GameObject fireball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fireball.name = "BigBossPhaseOneFireball";
        fireball.transform.position = spawnPos;
        fireball.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        fireball.transform.localScale = Vector3.one * 0.75f;

        Renderer renderer = fireball.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = CreateRuntimeMaterial(new Color(1f, 0.28f, 0.02f, 1f));
        }

        Collider collider = fireball.GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;

        TrailRenderer trail = fireball.AddComponent<TrailRenderer>();
        trail.time = 0.24f;
        trail.startWidth = 0.48f;
        trail.endWidth = 0f;
        trail.material = CreateRuntimeMaterial(new Color(1f, 0.18f, 0.02f, 0.88f));

        Light light = fireball.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.32f, 0.04f, 1f);
        light.range = 3.4f;
        light.intensity = 2.4f;

        BossProjectile projectile = fireball.AddComponent<BossProjectile>();
        projectile.speed = phaseOneProjectileSpeed;
        projectile.lifeTime = phaseOneProjectileLifeTime;
        projectile.damage = phaseOneProjectileDamage;
    }

    private Material CreateRuntimeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        material.SetColor("_BaseColor", color);
        material.SetColor("_EmissionColor", color);
        if (color.a < 1f)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }
        return material;
    }

    void SummonMinions()
    {
        if (minionPrefab != null)
        {
            // 🌟 一次在 Boss 的左前與右前方各召喚一隻小怪！
            Vector3 leftPos = transform.position + new Vector3(-2f, -1f, -3f);
            Vector3 rightPos = transform.position + new Vector3(2f, -1f, -3f);

            DisableMinionChestDrop(Instantiate(minionPrefab, leftPos, Quaternion.identity));
            DisableMinionChestDrop(Instantiate(minionPrefab, rightPos, Quaternion.identity));
        }
    }

    private void DisableMinionChestDrop(GameObject minion)
    {
        if (minion == null) return;

        Enemy enemy = minion.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.chestPrefab = null;
        }
    }
}
