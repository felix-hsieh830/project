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
    public float damagePercent = 0.1f;
    public float attackInterval = 1.35f;
    public float spawnHeight = 0.45f;

    [Header("三爪追蹤")]
    public float clawSpeed = 13.5f;
    public float clawHomingDuration = 0.85f;
    public float clawTurnSpeed = 8.5f;
    public float clawStopHomingDistance = 4.5f;
    public float clawAngle = 14f;

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
                attackInterval = 1.35f;
                activeDistance = 72f;
                damage = 18;
                break;
            case AttackPattern.FireballLine:
                attackInterval = 1.55f;
                activeDistance = 78f;
                damage = 16;
                break;
            case AttackPattern.ThornLanes:
                attackInterval = 2.1f;
                activeDistance = 82f;
                damage = 22;
                break;
        }
    }

    private void ShootClawTriple()
    {
        Vector3 spawnPos = transform.position + new Vector3(0f, spawnHeight, -1.2f);
        Vector3 targetDir = GetFlatDirectionToPlayer(spawnPos);

        for (int i = -1; i <= 1; i++)
        {
            Vector3 dir = Quaternion.Euler(0f, i * clawAngle, 0f) * targetDir;
            GameObject claw = CreateProjectileObject("ClawProjectile", spawnPos, Color.magenta, PrimitiveType.Capsule, new Vector3(0.22f, 0.22f, 0.9f));
            claw.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            SmallBossProjectile projectile = claw.AddComponent<SmallBossProjectile>();
            projectile.Setup(player.transform, dir, clawSpeed, damagePercent, 4.2f, clawHomingDuration, clawTurnSpeed, clawStopHomingDistance, true);
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

        TrailRenderer trail = fireball.AddComponent<TrailRenderer>();
        trail.time = 0.18f;
        trail.startWidth = 0.34f;
        trail.endWidth = 0f;
        trail.material = CreateRuntimeMaterial(new Color(1f, 0.22f, 0.02f, 0.85f));

        SmallBossProjectile projectile = fireball.AddComponent<SmallBossProjectile>();
        projectile.Setup(player.transform, dir, fireballSpeed, damagePercent, 3.5f, 0f, 0f, 0f, false);
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
            hazard.GetComponent<Renderer>().material = CreateRuntimeMaterial(new Color(1f, 0.06f, 0.02f, 0.55f));

            Collider collider = hazard.GetComponent<Collider>();
            if (collider != null) collider.isTrigger = true;

            SmallBossLaneHazard laneHazard = hazard.AddComponent<SmallBossLaneHazard>();
            laneHazard.Setup(damagePercent, thornWarningTime, thornActiveTime);
        }
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
