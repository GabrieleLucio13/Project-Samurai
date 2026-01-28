using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float activeTime = 0.08f;

    [Header("References")]
    [SerializeField] private GameObject hitVFX;

    private Collider col;
    private bool hasHit = false;

    private void Awake()
    {
        col = GetComponent<Collider>();
        col.enabled = false;

        if (hitVFX != null)
            hitVFX.SetActive(false);
    }

    private void OnEnable()
    {
        hasHit = false;
        col.enabled = true;
        Invoke(nameof(DisableHitbox), activeTime);
    }

    private void DisableHitbox()
    {
        col.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        hasHit = true;

        enemy.TakeDamage(damage);

        if (hitVFX != null)
        {
            hitVFX.SetActive(true);
        }
        DisableHitbox();
    }
}


