using System.Collections;
using UnityEngine;

public class SmallBossLaneHazard : MonoBehaviour
{
    private float damagePercent;
    private float warningTime;
    private float activeTime;
    private bool isActive;
    private bool hasHit;
    private Renderer hazardRenderer;

    public void Setup(float damagePercent, float warningTime, float activeTime)
    {
        this.damagePercent = damagePercent;
        this.warningTime = warningTime;
        this.activeTime = activeTime;
    }

    void Start()
    {
        hazardRenderer = GetComponent<Renderer>();
        StartCoroutine(HazardRoutine());
    }

    private IEnumerator HazardRoutine()
    {
        yield return new WaitForSeconds(warningTime);

        isActive = true;
        transform.localScale = new Vector3(transform.localScale.x, 1.25f, transform.localScale.z);
        transform.position += new Vector3(0f, 0.55f, 0f);

        if (hazardRenderer != null)
        {
            hazardRenderer.material.color = new Color(0.45f, 0.08f, 0.02f, 1f);
            hazardRenderer.material.SetColor("_BaseColor", new Color(0.45f, 0.08f, 0.02f, 1f));
        }

        yield return new WaitForSeconds(activeTime);
        Destroy(gameObject);
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
        int damage = Mathf.Max(1, Mathf.CeilToInt(player.maxHp * damagePercent));
        player.TakeDamage(damage);
    }
}
