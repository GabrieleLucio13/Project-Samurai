using UnityEngine;

public class EnemyCollider : MonoBehaviour
{
    public int damage;
    private bool hasHit;
    private Enemy enemyOwner;
    private void Awake()
    {
        enemyOwner = GetComponentInParent<Enemy>();
    }

    private void OnEnable()
    {
        hasHit = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        PlayerStatus status = other.GetComponentInParent<PlayerStatus>();
        if (status != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            
            status.TakeDamage(damage, hitPoint);
            hasHit = true;
            enemyOwner?.OnAttackHit();
        }
    }
}