using UnityEngine;

public class SmallBossProjectile : MonoBehaviour
{
    private Transform target;
    private Vector3 direction;
    private float speed;
    private int damage;
    private float lifeTime;
    private float homingTime;
    private float turnSpeed;
    private float stopHomingDistance;
    private Vector3 targetOffset;
    private Vector3 visualRotationOffset;
    private float hitRadius;
    private bool rotateVisual;
    private bool hasHit;

    public void Setup(Transform target, Vector3 startDirection, float speed, int damage, float lifeTime, float homingTime, float turnSpeed, float stopHomingDistance, Vector3 targetOffset, Vector3 visualRotationOffset, float hitRadius, bool rotateVisual)
    {
        this.target = target;
        this.direction = startDirection.normalized;
        this.speed = speed;
        this.damage = damage;
        this.lifeTime = lifeTime;
        this.homingTime = homingTime;
        this.turnSpeed = turnSpeed;
        this.stopHomingDistance = stopHomingDistance;
        this.targetOffset = targetOffset;
        this.visualRotationOffset = visualRotationOffset;
        this.hitRadius = hitRadius;
        this.rotateVisual = rotateVisual;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (homingTime > 0f && target != null)
        {
            Vector3 desired = target.position + targetOffset + new Vector3(0f, 0.9f, 0f) - transform.position;
            desired.y = 0f;
            if (stopHomingDistance > 0f && desired.sqrMagnitude <= stopHomingDistance * stopHomingDistance)
            {
                homingTime = 0f;
            }
            else if (desired.sqrMagnitude > 0.01f)
            {
                direction = Vector3.RotateTowards(direction, desired.normalized, turnSpeed * Time.deltaTime, 0f).normalized;
                homingTime -= Time.deltaTime;
            }
        }

        float moveDistance = speed * Time.deltaTime;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, hitRadius, direction, out hit, moveDistance, ~0, QueryTriggerInteraction.Collide))
        {
            if (TryHitPlayer(hit.collider))
            {
                return;
            }
        }

        transform.position += direction * moveDistance;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(visualRotationOffset);
        }

        if (rotateVisual)
        {
            transform.Rotate(0f, 0f, 360f * Time.deltaTime, Space.Self);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        TryHitPlayer(other);
    }

    private bool TryHitPlayer(Collider other)
    {
        if (hasHit) return false;

        PlayerStats player = other.GetComponentInParent<PlayerStats>();
        if (player == null) return false;

        hasHit = true;
        player.TakeDamage(Mathf.Max(1, damage));
        Destroy(gameObject);
        return true;
    }
}
