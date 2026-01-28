using UnityEngine;

public class AirEnemy : Enemy
{
    private enum EnemyState
    {
        Patrol,
        Chase,
        Stunned
    }

    [Header("Orbita")]
    public float fixedHeight = 4f;
    public float chaseSpeed = 10f;
    public Transform patrolCenter;
    public float orbitRadiusX = 8f;
    public float orbitRadiusZ = 4f;
    public float orbitAngularSpeed = 2f;            
    public float orbitResponsiveness = 1f;

    [Header("Rilevamento")]
    public float detectionRange = 10f;
    public float loseTargetRange = 14f;
    
    [Header("Attacco")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireCooldown = 2f;
    
    [Header("Stun")]
    [SerializeField] private float stunDuration = 1f;
    private float stunTimer;
    private EnemyState currentState = EnemyState.Patrol;
    private float orbitAngle;
    private float fireTimer;

    protected override void Start()
    {
        base.Start();

        rb.position = new Vector3(
            rb.position.x,
            fixedHeight,
            rb.position.z
        );
    }
    protected override void EnemyPhysicsUpdate()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                Orbit(patrolCenter.position);
                CheckForPlayer();
                break;

            case EnemyState.Chase:
                Orbit(playerPosition.position);
                HandleShooting();
                CheckLosePlayer();
                break;
            case EnemyState.Stunned:
                HandleStun();
                break;
        }
    }
    protected override void PlayDamageEffect()
    {
        base.PlayDamageEffect();

        currentState = EnemyState.Stunned;
        stunTimer = stunDuration;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(Vector3.down * 8f, ForceMode.Impulse);
    }

   //vola
   private void Orbit(Vector3 center)
    {
        orbitAngle += orbitAngularSpeed * Time.fixedDeltaTime;

        Vector3 orbitOffset = new Vector3(
            Mathf.Cos(orbitAngle) * orbitRadiusX,
            0f,
            Mathf.Sin(orbitAngle) * orbitRadiusZ
        );

        Vector3 desiredPosition = new Vector3(
            center.x + orbitOffset.x,
            fixedHeight,
            center.z + orbitOffset.z
        );

        Vector3 nextPosition = Vector3.Lerp(
            rb.position,
            desiredPosition,
            orbitResponsiveness * Time.fixedDeltaTime
        );

        rb.MovePosition(
            Vector3.MoveTowards(
                rb.position,
                nextPosition,
                moveSpeed * Time.fixedDeltaTime
            )
        );

        RotateTowards(center);
    }

    private void RotateTowards(Vector3 target)
    {
        Vector3 lookDir = target - rb.position;
        lookDir.y = 0f;

        if (lookDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            rb.MoveRotation(
                Quaternion.Slerp(
                    rb.rotation,
                    targetRotation,
                    5f * Time.fixedDeltaTime
                )
            );
        }
    }

    //rileva
    private void CheckForPlayer()
    {
        float distance = Vector3.Distance(rb.position, playerPosition.position);
        if (distance <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }
    }

    private void CheckLosePlayer()
    {
        float distance = Vector3.Distance(rb.position, playerPosition.position);
        if (distance > loseTargetRange)
        {
            currentState = EnemyState.Patrol;
        }
    }

   //attacca
    private void HandleShooting()
    {
        if (fireTimer > 0f)
        {
            fireTimer -= Time.fixedDeltaTime;
            return;
        }

        Shoot();
        fireTimer = fireCooldown;
    }

    private void Shoot()
    {
        Vector3 direction = (playerPosition.position - firePoint.position).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject projectileInstance = Instantiate(projectilePrefab, firePoint.position, rotation);
    }

    private void HandleStun()
    {
        stunTimer -= Time.fixedDeltaTime;

        if (stunTimer <= 0f)
        {
            currentState = EnemyState.Chase;

            rb.position = new Vector3(
                rb.position.x,
                fixedHeight,
                rb.position.z
            );
        }
    }
}

