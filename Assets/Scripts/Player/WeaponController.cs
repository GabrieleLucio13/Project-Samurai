using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Arma")]
    [SerializeField] private GameObject sword;
    [SerializeField] private Transform handSocket;
    [SerializeField] private Transform hipSocket;

    [Header("Shuriken")]
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private Transform shurikenSpawnPoint;
    [SerializeField] private LockOnSystem lockOnSystem;

    [Header("Fendente")]
    [SerializeField] private GameObject slashPrefab;
    [SerializeField] private Transform attackPoint;

    public void Equip()
    {
        sword.transform.SetParent(handSocket);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;
    }

    public void Unequip()
    {
        sword.transform.SetParent(hipSocket);
        sword.transform.localPosition = Vector3.zero;
        sword.transform.localRotation = Quaternion.identity;
    }

    public void SpawnSlash()
    {
        if (slashPrefab == null) return;

        var slash = Instantiate(
            slashPrefab,
            attackPoint.position,
            attackPoint.rotation
        );
        var hitbox = slash.GetComponentInChildren<WeaponHitbox>();
        if (hitbox != null)
            hitbox.gameObject.SetActive(true);

        Destroy(slash, 1.5f); 
    }

    public void SpawnShuriken()
    {
        if (shurikenPrefab == null || shurikenSpawnPoint == null)
            return;

        GameObject shuriken = Instantiate(
            shurikenPrefab,
            shurikenSpawnPoint.position,
            shurikenSpawnPoint.rotation
        );

        Projectile projectile = shuriken.GetComponent<Projectile>();
        if (projectile == null) return;

        projectile.owner = Projectile.ProjectileOwner.Player;

        if (lockOnSystem != null && lockOnSystem.CurrentTarget != null)
        {
            projectile.SetTarget(lockOnSystem.CurrentTarget);
        }
    }

}


