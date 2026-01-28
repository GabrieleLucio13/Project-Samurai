using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;

public class LockOnSystem : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera freeCamera;
    [SerializeField] private CinemachineCamera lockOnCamera;

    [Header("Target Settings")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float searchRadius = 15f;
    private bool isLockedOn ;
    private Transform currentTarget;
    private Enemy currentEnemy;
    private List<Transform> availableTargets = new();
    private Camera mainCamera;


    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleLockOn();

        if (!isLockedOn) return;

        if (currentTarget == null)
        {
            DisableLockOn();
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
            SwitchTarget(1);
    }

    private void ToggleLockOn()
    {
        if (isLockedOn)
            DisableLockOn();
        else
            EnableLockOn();
    }

    private void EnableLockOn()
    {
        FindTargets();

        if (availableTargets.Count == 0)
            return;

        SetTarget(availableTargets[0]);

        lockOnCamera.Priority = 20;
        freeCamera.Priority = 0;

        isLockedOn = true;
    }
    private void SetTarget(Transform target)
    {
        currentEnemy = target.GetComponent<Enemy>();

        if (currentEnemy != null && currentEnemy.healthBar != null)
            currentEnemy.healthBar.gameObject.SetActive(true);

        currentTarget = target;
        lockOnCamera.LookAt = currentTarget;
    }


   private void DisableLockOn()
    {
        if (currentEnemy != null && currentEnemy.healthBar != null)
            currentEnemy.healthBar.gameObject.SetActive(false);

        currentTarget = null;
        currentEnemy = null;
        availableTargets.Clear();

        freeCamera.Priority = 20;
        lockOnCamera.Priority = 0;

        isLockedOn = false;
    }


    private void FindTargets()
    {
        availableTargets.Clear();

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            searchRadius,
            enemyLayer
        );

        foreach (var hit in hits)
            availableTargets.Add(hit.transform);

        availableTargets = availableTargets
            .OrderBy(t => Vector3.Distance(transform.position, t.position))
            .ToList();
    }

    private void SwitchTarget(int direction)
    {
        if (currentEnemy != null && currentEnemy.healthBar != null)
            currentEnemy.healthBar.gameObject.SetActive(false);

        if (availableTargets.Count <= 1)
            return;

        int index = availableTargets.IndexOf(currentTarget);
        index = (index + direction + availableTargets.Count) % availableTargets.Count;

        SetTarget(availableTargets[index]);
    }

    public Transform CurrentTarget
    {
        get { return currentTarget; }
    }
    
    public bool IsLockedOn
    {
        get { return isLockedOn; }
    }
}


