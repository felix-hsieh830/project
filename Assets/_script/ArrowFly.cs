using UnityEngine;

public class ArrowFly : MonoBehaviour
{
    public float speed = 15f;
    private float totalSpeed;
    private float finalDamage;
    private float yawDegreesPerSecond = 0f;

    public void Setup(float playerDamage, float playerRange, float critRate, float critDamage, float inheritedSpeed, float flightSpeedMultiplier, float yawRate = 0f)
    {
        if (Random.value <= critRate)
        {
            finalDamage = playerDamage * critDamage;
            transform.localScale *= 1.8f;
        }
        else
        {
            finalDamage = playerDamage;
        }

        float baseSpeed = speed * flightSpeedMultiplier;
        totalSpeed = baseSpeed + inheritedSpeed;

        float finalRange = Mathf.Min(playerRange, 90f);
        float lifeTime = finalRange / totalSpeed;
        Destroy(gameObject, lifeTime);

        yawDegreesPerSecond = yawRate;
    }

    void Update()
    {
        if (yawDegreesPerSecond != 0f)
        {
            transform.Rotate(0f, yawDegreesPerSecond * Time.deltaTime, 0f);
        }

        float stepDistance = totalSpeed * Time.deltaTime;
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, stepDistance))
        {
            Enemy target = hit.collider.GetComponent<Enemy>();
            if (target != null)
            {
                target.TakeDamage(finalDamage);
                Destroy(gameObject);
                return;
            }
            BossHealth boss = hit.collider.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage(finalDamage);
                Destroy(gameObject);
                return;
            }
        }

        transform.Translate(0, 0, stepDistance);
    }

    void OnTriggerEnter(Collider other)
    {
        Enemy target = other.GetComponent<Enemy>();
        if (target != null)
        {
            target.TakeDamage(finalDamage);
            Destroy(gameObject);
            return;
        }
        BossHealth boss = other.GetComponent<BossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(finalDamage);
            Destroy(gameObject);
            return;
        }
    }
}