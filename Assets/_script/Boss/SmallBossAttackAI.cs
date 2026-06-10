using UnityEngine;

public class SmallBossAttackAI : MonoBehaviour
{
    public enum AttackPattern { ClawTriple, FireballLine, ThornLanes }

    [Header("攻擊型態")]
    public AttackPattern attackPattern = AttackPattern.ClawTriple;

    [Header("啟動距離")]
    public float activeDistance = 72f;
    public float stopAttackingBehindDistance = 4f;

    [Header("共用設定")]
    public int damage = 18;
    public float attackInterval = 1.35f;
    public float spawnHeight = 0.45f;

    [Header("三爪追蹤")]
    public float clawSpeed = 13.5f;
    public float clawHomingDuration = 0.85f;
    public float clawTurnSpeed = 8.5f;
    public float clawStopHomingDistance = 4.5f;
    public float clawAngle = 18f;
    public float clawTargetSpread = 1.6f;
    public float clawHitRadius = 0.26f;
    public Color clawBodyColor = new Color(0.35f, 0.08f, 0.95f, 1f);
    public Color clawTrailColor = new Color(0.85f, 0.22f, 1f, 0.85f);
    public GameObject clawProjectilePrefab;
    public Vector3 clawPrefabRotationOffset = Vector3.zero;
    public Vector3 clawPrefabScale = Vector3.one;

    [Header("直線火球")]
    public float fireballSpawnHeight = 0.15f;
    public float fireballSpeed = 18f;
    public int fireballBurstCount = 3;
    public float fireballBurstSpacing = 0.18f;

    [Header("地刺封路")]
    public float thornWarningTime = 0.65f;
    public float thornActiveTime = 1.15f;
    public float thornLength = 22f;
    public float thornLaneWidth = 2.25f;
    public float thornBossForwardOffset = 14f;

    private PlayerStats player;
    private BossHealth bossHealth;
    private float attackTimer;
    private int fireballBurstRemaining;
    private float fireballBurstTimer;
    private static readonly float[] Lanes = { -2.8f, 0f, 2.8f };

    void Start()
    {
        player = FindAnyObjectByType<PlayerStats>();
        bossHealth = GetComponent<BossHealth>();
        attackTimer = Random.Range(0.25f, attackInterval);
    }

    void Update()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerStats>();
            if (player == null) return;
        }

        if (bossHealth != null && (bossHealth.isInvincible || bossHealth.currentHp <= 0f)) return;

        float zDistance = transform.position.z - player.transform.position.z;
        if (zDistance > activeDistance || zDistance < -stopAttackingBehindDistance) return;

        TickFireballBurst();

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        switch (attackPattern)
        {
            case AttackPattern.ClawTriple:
                ShootClawTriple();
                break;
            case AttackPattern.FireballLine:
                StartFireballBurst();
                break;
            case AttackPattern.ThornLanes:
                SpawnThornLanes();
                break;
        }

        attackTimer = attackInterval;
    }

    public void SetPattern(AttackPattern pattern)
    {
        attackPattern = pattern;

        switch (attackPattern)
        {
            case AttackPattern.ClawTriple:
                attackInterval = 1.75f;
                activeDistance = 66f;
                damage = 12;
                break;
            case AttackPattern.FireballLine:
                attackInterval = 1.95f;
                activeDistance = 70f;
                damage = 10;
                break;
            case AttackPattern.ThornLanes:
                attackInterval = 2.55f;
                activeDistance = 74f;
                damage = 14;
                break;
        }
    }

    public void ApplyEncounterScaling(int encounterIndex)
    {
        float speedMultiplier;
        if (encounterIndex <= 1) speedMultiplier = 0.55f;
        else if (encounterIndex == 2) speedMultiplier = 0.7f;
        else if (encounterIndex == 3) speedMultiplier = 0.85f;
        else speedMultiplier = Mathf.Min(1.12f, 0.95f + (encounterIndex - 4) * 0.05f);

        attackInterval /= speedMultiplier;
        fireballBurstSpacing /= speedMultiplier;
        clawSpeed *= Mathf.Lerp(0.78f, 1.02f, Mathf.InverseLerp(1f, 6f, encounterIndex));
        fireballSpeed *= Mathf.Lerp(0.78f, 1.02f, Mathf.InverseLerp(1f, 6f, encounterIndex));
        damage = Mathf.Max(1, Mathf.RoundToInt(damage * Mathf.Lerp(0.75f, 1f, Mathf.InverseLerp(1f, 5f, encounterIndex))));
    }

    private void ShootClawTriple()
    {
        Vector3 spawnPos = transform.position + new Vector3(0f, spawnHeight, -1.2f);
        Vector3 targetDir = GetFlatDirectionToPlayer(spawnPos);

        for (int i = -1; i <= 1; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, i * clawAngle, 0f) * targetDir;
            GameObject claw = CreateClawProjectile(spawnPos, dir);

            SmallBossProjectile projectile = claw.GetComponent<SmallBossProjectile>();
            if (projectile == null)
            {
                projectile = claw.AddComponent<SmallBossProjectile>();
            }
            Vector3 targetOffset = new Vector3(i * clawTargetSpread, 0f, 0f);
            bool spinVisual = clawProjectilePrefab == null;
            projectile.Setup(player.transform, dir, clawSpeed, damage, 4.2f, clawHomingDuration, clawTurnSpeed, clawStopHomingDistance, targetOffset, clawPrefabRotationOffset, clawHitRadius, spinVisual);
        }
    }

    private void StartFireballBurst()
    {
        fireballBurstRemaining = Mathf.Max(1, fireballBurstCount);
        fireballBurstTimer = 0f;
        TickFireballBurst();
    }

    private void TickFireballBurst()
    {
        if (fireballBurstRemaining <= 0) return;

        fireballBurstTimer -= Time.deltaTime;
        if (fireballBurstTimer > 0f) return;

        ShootFireball();
        fireballBurstRemaining--;
        fireballBurstTimer = fireballBurstSpacing;
    }

    private void ShootFireball()
    {
        Vector3 spawnPos = transform.position + new Vector3(0f, fireballSpawnHeight, -1.25f);
        Vector3 dir = GetFlatDirectionToPlayer(spawnPos);

        GameObject fireball = CreateProjectileObject("FireballProjectile", spawnPos, new Color(1f, 0.32f, 0.05f, 1f), PrimitiveType.Sphere, Vector3.one * 0.55f);
        fireball.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        AddFireballVfx(fireball);

        SmallBossProjectile projectile = fireball.AddComponent<SmallBossProjectile>();
        projectile.Setup(player.transform, dir, fireballSpeed, damage, 3.5f, 0f, 0f, 0f, Vector3.zero, Vector3.zero, 0.65f, false);
    }

    private void SpawnThornLanes()
    {
        int safeLane = Random.Range(0, Lanes.Length);
        float baseZ = transform.position.z - thornBossForwardOffset;

        for (int i = 0; i < Lanes.Length; i++)
        {
            if (i == safeLane) continue;

            Vector3 pos = new Vector3(Lanes[i], 0.05f, baseZ);
            GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hazard.name = "ThornLaneWarning";
            hazard.transform.position = pos;
            hazard.transform.localScale = new Vector3(thornLaneWidth, 0.08f, thornLength);
            hazard.GetComponent<Renderer>().material = CreateRuntimeMaterial(new Color(0.28f, 0.85f, 0.2f, 0.45f));

            Collider collider = hazard.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;

            SmallBossLaneHazard laneHazard = hazard.AddComponent<SmallBossLaneHazard>();
            laneHazard.Setup(damage, thornWarningTime, thornActiveTime);
        }
    }

    private GameObject CreateClawProjectile(Vector3 spawnPos, Vector3 direction)
    {
        GameObject claw;
        if (clawProjectilePrefab != null)
        {
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(clawPrefabRotationOffset);
            claw = Instantiate(clawProjectilePrefab, spawnPos, rotation);
            claw.name = "ClawProjectile";
            claw.transform.localScale = Vector3.Scale(claw.transform.localScale, clawPrefabScale);
            EnsureTriggerColliders(claw);
        }
        else
        {
            claw = CreateProjectileObject("ClawProjectile", spawnPos, clawBodyColor, PrimitiveType.Capsule, new Vector3(0.22f, 0.22f, 0.9f));
            claw.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        if (claw.GetComponent<TrailRenderer>() == null)
        {
            AddTrail(claw, clawTrailColor, 0.22f, 0.16f);
        }

        return claw;
    }

    private void EnsureTriggerColliders(GameObject target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        SphereCollider sphere = target.GetComponent<SphereCollider>();
        if (sphere == null)
        {
            sphere = target.AddComponent<SphereCollider>();
        }

        sphere.radius = clawHitRadius;
        sphere.center = Vector3.zero;
        sphere.isTrigger = true;
        sphere.enabled = true;
    }

    private void AddTrail(GameObject target, Color color, float startWidth, float time)
    {
        TrailRenderer trail = target.AddComponent<TrailRenderer>();
        trail.time = time;
        trail.startWidth = startWidth;
        trail.endWidth = 0f;
        trail.material = CreateRuntimeMaterial(color);
    }

    private void AddFireballVfx(GameObject fireball)
    {
        AddTrail(fireball, new Color(1f, 0.22f, 0.02f, 0.9f), 0.42f, 0.24f);

        Light light = fireball.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.35f, 0.06f, 1f);
        light.range = 3.2f;
        light.intensity = 2.2f;
    }

    private Vector3 GetFlatDirectionToPlayer(Vector3 from)
    {
        Vector3 target = player.transform.position + new Vector3(0f, 0.9f, 0f);
        Vector3 dir = target - from;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) dir = Vector3.back;
        return dir.normalized;
    }

    private GameObject CreateProjectileObject(string objectName, Vector3 position, Color color, PrimitiveType primitiveType, Vector3 scale)
    {
        GameObject projectile = GameObject.CreatePrimitive(primitiveType);
        projectile.name = objectName;
        projectile.transform.position = position;
        projectile.transform.localScale = scale;
        projectile.GetComponent<Renderer>().material = CreateRuntimeMaterial(color);

        Collider collider = projectile.GetComponent<Collider>();
        if (collider != null) collider.isTrigger = true;

        return projectile;
    }

    private Material CreateRuntimeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        material.SetColor("_BaseColor", color);
        material.SetColor("_EmissionColor", color);
        return material;
    }
}
