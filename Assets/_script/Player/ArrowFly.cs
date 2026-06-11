using UnityEngine;

using System.Collections;

public class ArrowFly : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [Header("箭矢飛行速度")]
    public float speed = 15f;
    public float lifeTime = 3f;
    public float minLifeTime = 0.12f;
    public float hitRadius = 0.12f;

    [Header("箭矢外觀")]
    public bool disableColorAnimator = true;
    public float colorFadeDuration = 0.12f;

    private float totalSpeed;
    private float finalDamage;
    private float yawDegreesPerSecond = 0f;
    private bool isSetup = false;
    private bool hasHit = false;
    private PlayerStats playerStats;
    private Coroutine colorFadeRoutine;

    public void ApplyVisualColor(Color color)
    {
        ApplyVisualColor(color, color, false);
    }

    public void ApplyVisualColor(Color targetColor, Color startColor, bool animate)
    {
        ApplyVisualColor(targetColor, startColor, targetColor, startColor, animate);
    }

    public void ApplyVisualColor(Color targetColor, Color startColor, Color targetEmissionColor, Color startEmissionColor, bool animate)
    {
        StopColorAnimator();

        if (colorFadeRoutine != null)
        {
            StopCoroutine(colorFadeRoutine);
        }

        if (animate && colorFadeDuration > 0f)
        {
            ApplyColorToRenderers(startColor, startEmissionColor);
            colorFadeRoutine = StartCoroutine(FadeVisualColor(startColor, targetColor, startEmissionColor, targetEmissionColor));
            return;
        }

        ApplyColorToRenderers(targetColor, targetEmissionColor);
    }

    private IEnumerator FadeVisualColor(Color startColor, Color targetColor, Color startEmissionColor, Color targetEmissionColor)
    {
        float elapsed = 0f;
        while (elapsed < colorFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / colorFadeDuration);
            ApplyColorToRenderers(Color.Lerp(startColor, targetColor, t), Color.Lerp(startEmissionColor, targetEmissionColor, t));
            yield return null;
        }

        ApplyColorToRenderers(targetColor, targetEmissionColor);
        colorFadeRoutine = null;
    }

    private void StopColorAnimator()
    {
        if (!disableColorAnimator) return;

        Animator[] animators = GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].enabled = false;
            }
        }
    }

    private void ApplyColorToRenderers(Color color)
    {
        ApplyColorToRenderers(color, color);
    }

    private void ApplyColorToRenderers(Color color, Color emissionColor)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer targetRenderer = renderers[i];
            if (targetRenderer == null) continue;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            block.SetColor(EmissionColorId, emissionColor);
            targetRenderer.SetPropertyBlock(block);
        }
    }

    public void Setup(
        float playerDamage,
        float playerRange,
        float critRate,
        float critDamage,
        float inheritedSpeed,
        float flightSpeedMultiplier,
        float yawRate = 0f,
        PlayerStats stats = null
    )
    {
        isSetup = true;
        playerStats = stats;

        if (Random.value <= critRate)
        {
            finalDamage = playerDamage * critDamage;
        }
        else
        {
            finalDamage = playerDamage;
        }

        totalSpeed = (speed * flightSpeedMultiplier) + inheritedSpeed;

        if (totalSpeed <= 0f)
        {
            totalSpeed = speed;
        }

        lifeTime = Mathf.Max(minLifeTime, Mathf.Min(playerRange, 90f) / totalSpeed);
        Destroy(gameObject, lifeTime);

        yawDegreesPerSecond = yawRate;
    }

    void Start()
    {
        if (!isSetup)
        {
            totalSpeed = speed;
            Destroy(gameObject, lifeTime);
        }
    }

    void Update()
    {
        if (yawDegreesPerSecond != 0f)
        {
            transform.Rotate(0f, yawDegreesPerSecond * Time.deltaTime, 0f);
        }

        float stepDistance = totalSpeed * Time.deltaTime;

        RaycastHit hit;

        if (Physics.SphereCast(transform.position, hitRadius, transform.forward, out hit, stepDistance, ~0, QueryTriggerInteraction.Collide))
        {
            HandleCollision(hit.collider);
        }

        transform.Translate(0, 0, stepDistance);
    }

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    private void HandleCollision(Collider hitCollider)
    {
        if (hasHit)
        {
            return;
        }

        // 重點：改成抓父物件，避免 Collider 在子物件時抓不到 Enemy
        Enemy target = hitCollider.GetComponentInParent<Enemy>();
        BossHealth boss = hitCollider.GetComponentInParent<BossHealth>();

        if (target == null && boss == null)
        {
            return;
        }

        hasHit = true;

        if (!IsVisibleToMainCamera(hitCollider.bounds))
        {
            Destroy(gameObject);
            return;
        }

        bool dealtDamage = false;
        float lifestealDamage = 0f;

        if (target != null)
        {
            lifestealDamage = Mathf.Min(finalDamage, Mathf.Max(0f, target.CurrentHp));
            dealtDamage = target.TakeDamage(finalDamage);
            if (dealtDamage) SfxManager.Play("arrow_hit", 0.52f, 0.035f);
        }
        else if (boss != null)
        {
            lifestealDamage = Mathf.Min(finalDamage, Mathf.Max(0f, boss.currentHp));
            dealtDamage = boss.TakeDamage(finalDamage);
            if (dealtDamage) SfxManager.Play("boss_hit", 0.58f, 0.045f);
        }

        if (!dealtDamage)
        {
            Destroy(gameObject);
            return;
        }

        if (playerStats == null)
        {
            playerStats = FindAnyObjectByType<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogWarning("吸血失敗：找不到 PlayerStats");
            Destroy(gameObject);
            return;
        }

        if (playerStats.lifestealLevel <= 0)
        {
            Debug.Log("沒有觸發吸血：lifestealLevel 目前是 0");
            Destroy(gameObject);
            return;
        }

        float lifestealRate = playerStats.lifestealLevel * 0.05f;
        if (lifestealDamage <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        float healAmountFloat = lifestealDamage * lifestealRate;
        int healAmount = Mathf.Max(1, Mathf.FloorToInt(healAmountFloat));

        //Debug.Log("觸發吸血，等級：" + playerStats.lifestealLevel + "，回血量：" + healAmount);

        playerStats.Heal(healAmount, true);

        Destroy(gameObject);
    }

    private bool IsVisibleToMainCamera(Bounds bounds)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return true;
        }

        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        return IsPointInCameraView(mainCamera, center)
            || IsPointInCameraView(mainCamera, center + new Vector3(extents.x, extents.y, extents.z))
            || IsPointInCameraView(mainCamera, center + new Vector3(extents.x, extents.y, -extents.z))
            || IsPointInCameraView(mainCamera, center + new Vector3(extents.x, -extents.y, extents.z))
            || IsPointInCameraView(mainCamera, center + new Vector3(extents.x, -extents.y, -extents.z))
            || IsPointInCameraView(mainCamera, center + new Vector3(-extents.x, extents.y, extents.z))
            || IsPointInCameraView(mainCamera, center + new Vector3(-extents.x, extents.y, -extents.z))
            || IsPointInCameraView(mainCamera, center + new Vector3(-extents.x, -extents.y, extents.z))
            || IsPointInCameraView(mainCamera, center + new Vector3(-extents.x, -extents.y, -extents.z));
    }

    private bool IsPointInCameraView(Camera camera, Vector3 worldPoint)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(worldPoint);
        return viewportPoint.z > 0f
            && viewportPoint.x >= 0f
            && viewportPoint.x <= 1f
            && viewportPoint.y >= 0f
            && viewportPoint.y <= 1f;
    }
}
