using System.Collections;
using UnityEngine;

public class SmallBossLaneHazard : MonoBehaviour
{
    private int damage;
    private float warningTime;
    private float activeTime;
    private bool isActive;
    private bool hasHit;
    private Renderer hazardRenderer;
    private float laneWidth;
    private float laneLength;

    public void Setup(int damage, float warningTime, float activeTime)
    {
        this.damage = damage;
        this.warningTime = warningTime;
        this.activeTime = activeTime;
    }

    void Start()
    {
        hazardRenderer = GetComponent<Renderer>();
        laneWidth = transform.localScale.x;
        laneLength = transform.localScale.z;
        StartCoroutine(HazardRoutine());
    }

    private IEnumerator HazardRoutine()
    {
        yield return new WaitForSeconds(warningTime);

        isActive = true;
        transform.localScale = new Vector3(transform.localScale.x, 1.25f, transform.localScale.z);
        transform.position += new Vector3(0f, 0.55f, 0f);
        CreateVineVisuals(activeTime);

        if (hazardRenderer != null)
        {
            hazardRenderer.material.color = new Color(0.12f, 0.38f, 0.08f, 0.85f);
            hazardRenderer.material.SetColor("_BaseColor", new Color(0.12f, 0.38f, 0.08f, 0.85f));
        }

        yield return new WaitForSeconds(activeTime);
        Destroy(gameObject);
    }

    private void CreateVineVisuals(float lifetime)
    {
        int vineCount = 4;
        for (int i = 0; i < vineCount; i++)
        {
            float xOffset = Mathf.Lerp(-laneWidth * 0.35f, laneWidth * 0.35f, vineCount <= 1 ? 0.5f : (float)i / (vineCount - 1));
            Vector3 vinePos = transform.position + new Vector3(xOffset, 0.25f + i * 0.04f, 0f);

            GameObject vine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vine.name = "VineHazard";
            vine.transform.position = vinePos;
            vine.transform.rotation = Quaternion.Euler(90f, 0f, Random.Range(-10f, 10f));
            vine.transform.localScale = new Vector3(0.13f, laneLength * 0.5f, 0.13f);

            Renderer renderer = vine.GetComponent<Renderer>();
            if (renderer != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                Material material = new Material(shader);
                material.color = new Color(0.18f, 0.48f, 0.12f, 1f);
                material.SetColor("_BaseColor", new Color(0.18f, 0.48f, 0.12f, 1f));
                renderer.material = material;
            }

            Collider collider = vine.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;
            Destroy(vine, lifetime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        if (!isActive || hasHit) return;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return;

        hasHit = true;
        player.TakeDamage(Mathf.Max(1, damage));
    }
}
