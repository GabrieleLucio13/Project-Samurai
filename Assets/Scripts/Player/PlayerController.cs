using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 13f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float runStaminaCost = 1f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpCooldown = 2f;
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpStaminaCost = 20f;

    [Header("Schivata")]
    [SerializeField] private float dodgeSpeed = 9.5f;
    [SerializeField] private float dodgeCooldown = 0.8f;
    [SerializeField] private float dodgeStaminaCost = 20f;
    
    [Header("Combattimento")]
    [SerializeField] private int maxComboCount = 3;   
    [SerializeField] private float lockOnRotationSpeed = 8f;
    [SerializeField] private float parryWindow = 0.25f; 
    [SerializeField] private float attackStaminaCost = 10f;
    [SerializeField] private float shurikenCooldown = 0.6f;
    [SerializeField] private float combatExitDelay = 3f;

    [Header("Riferimenti")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Transform model;
    [SerializeField] private LockOnSystem lockOn;
    [SerializeField] private Renderer debugRenderer;

    private CharacterController _controller;
    private PlayerStatus _playerStatus;
    
    // input
    private Vector2 _moveInput;
    private bool _jumpRequested;
    private bool _dodgeRequested;
    private bool _attackRequested;
    private bool _defenseHeld;
    private bool _shurikenRequested;
    

    // movimento
    private Vector3 _movement;
    private bool _isMoving;
    private bool _isRunning;
    private bool _canRun;
    
    // stato
    private bool _isGrounded;
    private bool _isInvulnerable;
    
    //salto
    private Vector3 _airVelocity;
    private float _verticalVelocity;
    private float _jumpCooldownTimer;
    private float coyoteTimer;
    
    // schivata
    private bool _isDodging;
    private float _dodgeCooldownTimer;
    private Vector3 _dodgeDirection;
    

    // attacco
    private bool _isAttacking;
    private bool _canQueueCombo;
    private bool _attackQueued;
    private int _currentCombo;
    private float _shurikenCooldownTimer;

    // difesa
    private bool _isDefending;
    private bool _isParryActive;
    private float _parryTimer;
    
    // rinfodero
    private bool _isSheathing;
    private float _combatTimer;
    private bool _isInCombat;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _playerStatus = GetComponent<PlayerStatus>();
    }
    private void Update()
    {
        CheckGround();
        HandleLockRotation();
        UpdateAnimator();
    }
    private void FixedUpdate()
    {
        ProcessMovement();
        ProcessJump();
        ProcessDodge();
        ProcessAttack();
        ProcessDefense();
        ProcessShuriken();
        CheckCombat();
        ProcessGravity();     
        ApplyMovement();
    }

    //INPUT
    private void Start()
    {
        inputManager.Move += OnMove;
        inputManager.Jump += OnJump;
        inputManager.Dodge += OnDodge;
        inputManager.Attack += OnAttack;
        inputManager.Run += OnRun;
        inputManager.Defense += OnDefense;
        inputManager.Throw += OnThrow;
    }
    private void OnDestroy()
    {
        inputManager.Move -= OnMove;
        inputManager.Jump -= OnJump;
        inputManager.Dodge -= OnDodge;
        inputManager.Attack -= OnAttack;
        inputManager.Run -= OnRun;
        inputManager.Defense -= OnDefense;
        inputManager.Throw -= OnThrow;
    }
    private void OnMove(Vector2 value) => _moveInput = value;
    private void OnJump() => _jumpRequested = true;
    private void OnDodge() => _dodgeRequested = true;
    private void OnAttack() => _attackRequested = true;
    private void OnThrow() => _shurikenRequested = true;
    private void OnRun(bool running) => _isRunning = running;
    private void OnDefense(bool held)
    {
        _defenseHeld = held;

        if (held) StartDefense();
        else StopDefense();
    }
    
    //LOCK ON
    private void HandleLockRotation()
    {
        if (lockOn == null)
            return;

        if (!lockOn.IsLockedOn || lockOn.CurrentTarget == null)
            return;

        if (_movement.magnitude > 0.05f)
            return;

        if (!IsIdle())
            return;

        Vector3 direction = lockOn.CurrentTarget.position - model.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        model.rotation = Quaternion.Slerp(
            model.rotation,
            targetRotation,
            lockOnRotationSpeed * Time.deltaTime
        );
    }
    
    //FISICA
    private void CheckGround()
    {
        _isGrounded = _controller.isGrounded;

        if (_isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.fixedDeltaTime;

    }
    private void ProcessGravity()
    {
        if (_isGrounded && _verticalVelocity < 0)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += gravity * Time.fixedDeltaTime;
    }
    
    //ANIMAZIONI SALTO E MOVIMENTO
    private void UpdateAnimator()
    {
        _isMoving = _movement.magnitude > 0.1f;

        animator.SetBool("IsMoving", _isMoving);
        animator.SetBool("IsRunning", _canRun && _isGrounded);
        animator.SetBool("IsGrounded", _isGrounded);
        animator.SetFloat("VerticalVelocity", _verticalVelocity);
    }
    
    //MOVIMENTO
    private void ProcessMovement()
    {
        if (!CanMove())
        {
            _movement = Vector3.zero;
            return;
        }
            
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        _movement = cameraForward * _moveInput.y + cameraRight * _moveInput.x;
        _canRun = _isRunning
          && _movement.magnitude > 0.1f
          && !_isDodging
          && !_isAttacking
          && !_isDefending
          && _playerStatus.stamina > 0f;

        if (_movement.magnitude > 1f)
            _movement.Normalize();

        // ROTAZIONE
        if (_movement.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(_movement);
            model.rotation = Quaternion.Slerp(
                model.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }
    private void ApplyMovement()
    {
        Vector3 finalMovement;

        if (_isDodging)
        {
            finalMovement = _dodgeDirection * dodgeSpeed;
        }
        else if (!_isGrounded)
        {
            finalMovement = _airVelocity; 
        }
        else
        {
            if (_canRun)
            {
                bool hasStamina = _playerStatus.TryUseStamina(runStaminaCost * Time.deltaTime);

                if (!hasStamina)
                    _canRun = false;
            }

            finalMovement = _movement * (_canRun ? runSpeed : moveSpeed);
        }

        finalMovement.y = _verticalVelocity;
        _controller.Move(finalMovement * Time.fixedDeltaTime);
    }
    
    //ATTACCO
    private void ProcessAttack()
    {
        if (_attackRequested)
        {
            if (!_isAttacking && CanAttack())
            {
                StartAttack();
            }
            else if (_isAttacking && _canQueueCombo)
            {
                _attackQueued = true;
            }
        }
        _attackRequested = false;
    }
    private void StartAttack()
    {
        _isAttacking = true;
        _currentCombo++;

        animator.SetInteger("ComboIndex", _currentCombo);
        animator.SetTrigger("Attack");

    }
    private void EndAttack()
    {
        _isAttacking = false;
        _attackQueued = false;
        _canQueueCombo = false;
        _currentCombo = 0;

        animator.SetInteger("ComboIndex", 0);
    }
     public void Anim_AttackStart()
    {
        _attackQueued = false;
        _canQueueCombo = false;
    }
    public void Anim_ComboWindowOpen()
    {
        _canQueueCombo = true;
    }
    public void Anim_ComboWindowClose()
    {
        _canQueueCombo = false;
    }
    public void Anim_AttackEnd()
    {
        if (_attackQueued && _currentCombo < maxComboCount)
            StartAttack(); 
        else
            EndAttack();
    }


    //LANCIO
    private void ProcessShuriken()
    {
        if (_shurikenCooldownTimer > 0f)
            _shurikenCooldownTimer -= Time.fixedDeltaTime;

        if (_shurikenRequested && CanThrowShuriken())
        {
            ThrowShuriken();
        }

        _shurikenRequested = false;
    }
    private void ThrowShuriken()
    {
        _shurikenCooldownTimer = shurikenCooldown;
        animator.SetTrigger("throw");
    }

    //DIFESA
    private void ProcessDefense()
    {
        if (!_isDefending && !CanDefend())
            return;

        if (_parryTimer > 0f)
        {
            _parryTimer -= Time.fixedDeltaTime;
            if (_parryTimer <= 0f)
            {
                _isParryActive = false;
                _isInvulnerable = false;
                SetInvulnerable(false);
            }
        }
    }
    private void StartDefense()
    {
        if (!CanDefend())
            return;

        _isDefending = true;
        
        _isSheathing = false;
        _isAttacking = false;
        _currentCombo = 0;
        
        _parryTimer = parryWindow;
        _isParryActive = true;
        _isInvulnerable = true;
        SetInvulnerable(true);

        animator.SetBool("IsDefending", _isDefending);
    }
    private void StopDefense()
    {
        _isDefending = false;
        _isParryActive = false;

        animator.SetBool("IsDefending", _isDefending);
    }

    //SCHIVATA 
    private void ProcessDodge()
    {
        if (_dodgeCooldownTimer > 0f)
            _dodgeCooldownTimer -= Time.fixedDeltaTime;

        if (_dodgeRequested && _dodgeCooldownTimer <= 0f && CanDodge())
        {
            StartDodge();
        }

        _dodgeRequested = false;
    }   
    private void StartDodge()
    {
        _isDodging = true;
        SetInvulnerable(true);
        _playerStatus.SetRegenBlocked(true);

        _dodgeDirection = _movement.sqrMagnitude > 0.1f
            ? _movement.normalized
            : model.forward;

        _dodgeCooldownTimer = dodgeCooldown;
        animator.SetBool("IsDodging", _isDodging);
    }
    public void SetInvulnerable(bool value)
    {
        _isInvulnerable = value;

        if (debugRenderer != null)
            debugRenderer.material.color = value ? Color.red : Color.white;
    }
    public void Anim_DodgeEnd()
    {
        _isDodging = false;
        _playerStatus.SetRegenBlocked(false);

        animator.SetBool("IsDodging", _isDodging);
    }
    public void Anim_DodgeIFrameEnd()
    {
        SetInvulnerable(false);
    }
    //SALTO
    private void ProcessJump()
    {
        if (_jumpCooldownTimer > 0)
        {
            _jumpCooldownTimer -= Time.fixedDeltaTime;
        }

        if (_jumpRequested &&_jumpCooldownTimer <=0 && CanJump())
        {
            StartJump();
        }
        
        _jumpRequested = false;

    }
    private void StartJump()
    {
        _airVelocity = _movement * (_isRunning ? runSpeed : moveSpeed);
        _verticalVelocity = jumpForce;

        coyoteTimer = 0f;
        _jumpCooldownTimer = jumpCooldown;
 
    }
    
    //RINFODERO
    private void CheckCombat()
    {
        bool hasCombatAction = _isAttacking || _isDefending || _isParryActive || lockOn.IsLockedOn;

        if (hasCombatAction)
        {
            _combatTimer = combatExitDelay;
            _isInCombat = true;
            return;
        }

        if (_isInCombat)
        {
            _combatTimer -= Time.fixedDeltaTime;

            if (_combatTimer <= 0f && !_isSheathing)
            {
                _isInCombat = false;
                StartSheathing();
            }
        }
    }
    private void StartSheathing()
    {
        _isSheathing = true;
        animator.SetTrigger("Sheath");
    }
    public void Anim_SheathEnd()
    {
        _isSheathing = false;
    }
    //CONDIZIONI
    private bool IsIdle()
    {
        return _movement.magnitude < 0.05f &&
            !_isDodging &&
            !_isAttacking &&
            _isGrounded;
    }
    private bool CanMove()
    {
        return _isGrounded && !_isDodging;
    }
    private bool CanAttack()
    {
        return !_isDodging && !_isAttacking && !_isDefending &&!_isRunning && !_isSheathing && _playerStatus.TryUseStamina(attackStaminaCost);
    }
    private bool CanJump()
    {
        return _isGrounded && !_isDodging && !_isDefending && _playerStatus.TryUseStamina(jumpStaminaCost);
    }
    private bool CanDodge()
    {
        return _isGrounded && !_isDodging && !_isAttacking && !_isDefending && _playerStatus.TryUseStamina(dodgeStaminaCost);
    }
    private bool CanDefend()
    {
        return !_isDodging && !_isRunning && !_isSheathing;
    }
    private bool CanThrowShuriken()
    {
       return !_isDodging &&
            !_isAttacking &&
            !_isDefending &&
            !_isRunning &&
            !_isSheathing &&
            _shurikenCooldownTimer <= 0f &&
            _playerStatus.TryUseShuriken();
    }

}