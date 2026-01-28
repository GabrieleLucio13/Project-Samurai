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
            Debug.Log("[EnemyAttack] Player colpito!");
            status.TakeDamage(damage);
            hasHit = true;
            enemyOwner?.OnAttackHit();
        }
    }
}