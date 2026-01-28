using UnityEngine;

public class PatrolEnemy : Enemy
{
    private enum EnemyState
    {
        Patrol,
        Attack,
        Charge,
        Recoil
    }

    [Header("Movimento")]
    public float patrolSpeed = 3f;
    public float attackMoveSpeed = 4f;
    public float rotationSpeed = 5f;

    [Header("Carica")]
    [SerializeField] private float chargeSpeed = 15f;
    [SerializeField] private float chargeDuration = 0.8f;
    [SerializeField] private float chargeCooldown = 2f;
    [SerializeField] private float chargeOverDistance = 5f;
    [SerializeField] private GameObject attackHitbox;

    [Header("KnockBack")]
    [SerializeField] private float recoilForce = 8f;
    [SerializeField] private float recoilDuration = 0.3f;

    [Header("Rilevamento")]
    public float detectionRange = 15f;
    public float loseTargetRange = 12f;

    [Header("Riferimenti")]
    public Transform[] patrolPoints;

    private int currentPatrolIndex;
    private Vector3 chargeTarget;
    private float chargeTimer;
    private float chargeCooldownTimer;
    private float recoilTimer;
    private Vector3 recoilDirection;
    private EnemyState currentState = EnemyState.Patrol;

    protected override void EnemyPhysicsUpdate()
    {
        if (chargeCooldownTimer > 0f)
            chargeCooldownTimer -= Time.fixedDeltaTime;

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                CheckForPlayer();
                break;

            case EnemyState.Attack:
                Attack();
                break;

            case EnemyState.Charge:
                PerformCharge();
                break;
            case EnemyState.Recoil:
                PerformRecoil();
                break;
        }
    }
    protected override void PlayDamageEffect()
    {
        base.PlayDamageEffect();

        if (playerPosition == null)
            return;

        recoilDirection = (rb.position - playerPosition.position).normalized;
        recoilTimer = recoilDuration;

        currentState = EnemyState.Recoil;
    }

    // in pattuglia
    private void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        MoveTowards(targetPoint.position, patrolSpeed);

        if (Vector3.Distance(rb.position, targetPoint.position) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }
    private void CheckForPlayer()
    {
        if (playerPosition == null || chargeCooldownTimer > 0f)
            return;

        float distance = Vector3.Distance(rb.position, playerPosition.position);

        if (distance <= detectionRange)
        {
            currentState = EnemyState.Attack;
        }
    }

    //in combattimento
    private void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - rb.position).normalized;
        if (direction == Vector3.zero)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(
            Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime)
        );
    }
    private void MoveTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - rb.position).normalized;

        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(
                Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime)
            );
        }
    } 
    private void Attack()
    {
        if (playerPosition == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        float distance = Vector3.Distance(rb.position, playerPosition.position);

        if (distance > loseTargetRange)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        Vector3 direction = (playerPosition.position - rb.position).normalized;
        chargeTarget = playerPosition.position + direction * chargeOverDistance;

        FaceTarget(playerPosition.position);

        if (distance <= detectionRange && chargeCooldownTimer <= 0f)
        {
            chargeTimer = chargeDuration;
            attackHitbox.SetActive(true);
            currentState = EnemyState.Charge;
        }
    }

    //in carica
    private void PerformCharge()
    {
        if (chargeTimer <= 0f)
        {
            chargeCooldownTimer = chargeCooldown;
            attackHitbox.SetActive(false);
            currentState = EnemyState.Attack;
            return;
        }

        chargeTimer -= Time.fixedDeltaTime;

        MoveTowards(chargeTarget, chargeSpeed);
    }
    public override void OnAttackHit()
    {
        attackHitbox.SetActive(false);

        recoilDirection = -(playerPosition.position - rb.position).normalized;
        recoilTimer = recoilDuration;

        currentState = EnemyState.Recoil;
    }

    //in rinculo
    private void PerformRecoil()
    {
        if (recoilTimer <= 0f)
        {
            chargeCooldownTimer = chargeCooldown;
            currentState = EnemyState.Attack;
            return;
        }

        recoilTimer -= Time.fixedDeltaTime;
        rb.MovePosition(
            rb.position + recoilDirection * recoilForce * Time.fixedDeltaTime
        );
    }
    
}


