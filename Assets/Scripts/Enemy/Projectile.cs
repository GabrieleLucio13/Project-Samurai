using UnityEngine;

public class Projectile : MonoBehaviour
{
    public enum ProjectileOwner
    {
        Player,
        Enemy
    }

    [Header("Stats")]
    public int damage = 10;
    public float speed = 15f;
    public float lifeTime = 3f;
    public ProjectileOwner owner;

    [Header("Assist Aim")]
    [Range(0f, 1f)]
    public float assistStrength = 0.6f;

    private Vector3 moveDirection;
    private bool hasHit;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
        moveDirection = transform.forward;
    }
    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        switch (owner)
        {
            case ProjectileOwner.Player:
                TryHitEnemy(other);
                break;

            case ProjectileOwner.Enemy:
                TryHitPlayer(other);
                break;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
            return;

        Vector3 targetPoint = GetTargetPoint(newTarget);
        Vector3 directionToTarget = (targetPoint - transform.position).normalized;

        moveDirection = Vector3.Slerp(
            transform.forward,
            directionToTarget,
            assistStrength
        ).normalized;

        transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    private Vector3 GetTargetPoint(Transform target)
    {
        Collider col = target.GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds.center;
        }

        return target.position;
    } 

    private void TryHitPlayer(Collider other)
    {
        PlayerStatus status = other.GetComponentInParent<PlayerStatus>();
        if (status == null) return;

        status.TakeDamage(damage);
        Hit();
    }

    private void TryHitEnemy(Collider other)
    {
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        enemy.TakeDamage(damage);
        Hit();
    }

    private void Hit()
    {
        hasHit = true;
        Destroy(gameObject);
    }
}


