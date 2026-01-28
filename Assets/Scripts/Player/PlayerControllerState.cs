/* using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControlleState : MonoBehaviour
{
    public enum PlayerState
    {
        Idle = 0,
        Walk = 1,
        Run = 2,
        Jump = 3,
        Fall = 4,
        Dodge = 5,
        Attack = 6,
        Defense = 7
    }

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 13f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpCooldown = 2f;
    [SerializeField] private float coyoteTime = 0.15f;

    [Header("Schivata")]
    [SerializeField] private float dodgeSpeed = 9.5f;
    [SerializeField] private float dodgeDuration = 0.15f;
    [SerializeField] private float dodgeCooldown = 0.8f;
    
    
    [Header("Attacco")]
    [SerializeField] private float attackDuration = 0.35f;
    [SerializeField] private float comboWindow = 0.6f;  
    [SerializeField] private int maxComboCount = 3;   

    [Header("Difesa")]
    [SerializeField] private float parryWindow = 0.25f;      


    [Header("Riferimenti")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform model;
    [SerializeField] private WeaponController weaponController;

    private CharacterController _controller;
    private PlayerInputAction _inputActions;

    // input
    private Vector2 _moveInput;
    
    private bool _jumpRequested;
    private bool _dodgeRequested;

    // movement
    private Vector3 _movement;
    private bool _isRunning;
    
    //physics
    private bool _isGrounded;
    
    //jump
    private float _verticalVelocity;
    private float _jumpCooldownTimer;
    private float coyoteTimer;
    
    // Dodge
    private bool _isDodging;
    private float _dodgeTimer;
    private float _dodgeCooldownTimer;
    private Vector3 _dodgeDirection;

    //attack
    private bool _attackRequested;
    private bool _isAttacking;
    private float _attackTimer;
    private float _comboTimer;
    private bool _attackQueued;
    private int _currentCombo;

    //defense
    private bool _defenseHeld;
    private bool _isDefending;
    private bool _isParryActive;
    private float _parryTimer;

    // animation state
    private PlayerState _state;

    // cached input callbacks
    private System.Action<InputAction.CallbackContext> _runStarted;
    private System.Action<InputAction.CallbackContext> _runCanceled;
    private System.Action<InputAction.CallbackContext> _defenseStarted;
    private System.Action<InputAction.CallbackContext> _defenseCanceled;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _inputActions = new PlayerInputAction();

        _runStarted = ctx => _isRunning = true;
        _runCanceled = ctx => _isRunning = false;

        _defenseStarted = ctx => StartDefense();
        _defenseCanceled = ctx => StopDefense();
    }

    private void OnEnable()
    {
        _inputActions.Player.Enable();

        _inputActions.Player.Jump.performed += OnJump;
        _inputActions.Player.Dodge.performed += OnDodge;
        _inputActions.Player.Attack.performed += OnAttack;

        _inputActions.Player.Run.started += _runStarted;
        _inputActions.Player.Run.canceled += _runCanceled;

        _inputActions.Player.Defense.started += _defenseStarted;
        _inputActions.Player.Defense.canceled += _defenseCanceled;
    }

    private void OnDisable()
    {
        _inputActions.Player.Jump.performed -= OnJump;
        _inputActions.Player.Dodge.performed -= OnDodge;
        _inputActions.Player.Attack.performed -= OnAttack;

        _inputActions.Player.Run.started -= _runStarted;
        _inputActions.Player.Run.canceled -= _runCanceled;

        _inputActions.Player.Defense.started -= _defenseStarted;
        _inputActions.Player.Defense.canceled -= _defenseCanceled;

        _inputActions.Player.Disable();
    }

    private void OnJump(InputAction.CallbackContext ctx) => _jumpRequested = true;
    private void OnDodge(InputAction.CallbackContext ctx) => _dodgeRequested = true;
    private void OnAttack(InputAction.CallbackContext ctx) => _attackRequested = true;


    private void Update()
    {
        CheckGround();
        ReadMoveInput();
    }

    private void ReadMoveInput()
    {
        if(_isDodging || !_isGrounded)
        {
            return;
        }
        _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        
        ProcessMovement();
        ProcessJump();
        ProcessDodge();
        ProcessAttack();
        ProcessGravity();
        ProcessDefense();
        UpdateState();     
        ApplyMovement();
    }

    private void CheckGround()
    {
        _isGrounded = _controller.isGrounded;

        if (_isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.fixedDeltaTime;

    }

    private void ProcessMovement()
    {
        if (_isDefending)
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

        if (_movement.magnitude > 1f)
            _movement.Normalize();

        if (_movement.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(_movement.normalized);
            model.rotation = Quaternion.Slerp(model.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void ProcessJump()
    {
        if (_jumpCooldownTimer > 0)
        {
            _jumpCooldownTimer -= Time.fixedDeltaTime;
        }

        if (_jumpRequested && coyoteTimer > 0f &&_jumpCooldownTimer <=0 && !_isDodging && !_isDefending)
        {
            
            _verticalVelocity = jumpForce;
            _jumpRequested = false;
            coyoteTimer = 0f; 
            _jumpCooldownTimer = jumpCooldown;
        }
        
        _jumpRequested = false;

    }

    private void ProcessDodge()
    {
        if (_dodgeCooldownTimer > 0)
            _dodgeCooldownTimer -= Time.fixedDeltaTime;

        if (_dodgeRequested && _dodgeCooldownTimer <= 0 && !_isDodging && _isGrounded && !_isDefending)
        {
            _dodgeDirection = _movement.sqrMagnitude > 0.1f
                ? _movement.normalized
                : model.forward;

            _isDodging = true;
            _dodgeTimer = dodgeDuration;
            _dodgeCooldownTimer = dodgeCooldown;
        }

        _dodgeRequested = false;

        if (_isDodging)
        {
            _dodgeTimer -= Time.fixedDeltaTime;
            if (_dodgeTimer <= 0)
                _isDodging = false;
        }
    }

    private void ProcessAttack()
    {
        if (_isDefending)
        return;
    
        if (_attackRequested && !_isAttacking && !_isDodging && _isGrounded)
        {
            _isAttacking = true;
            _attackTimer = attackDuration;
            _comboTimer = comboWindow;
            _currentCombo++;
        }
        
        else if (_attackRequested && _isAttacking)
        {
            _attackQueued = true;
        }

        _attackRequested = false;

        if (!_isAttacking)
            return;

        _attackTimer -= Time.fixedDeltaTime;
        _comboTimer -= Time.fixedDeltaTime;


        if (_attackTimer <= 0f)
        {
            if (_attackQueued && _currentCombo < maxComboCount)
            {
                
                _attackQueued = false;
                ComboAttack();
            }
            else if (_comboTimer <= 0f)
            {
                _isAttacking = false;
                _currentCombo = 0;
            }
        }
    }

    private void ProcessDefense()
    {
        if (!_isDefending)
            return;

        if (_parryTimer > 0f)
        {
            _parryTimer -= Time.fixedDeltaTime;
            if (_parryTimer <= 0f)
            {
                _isParryActive = false;
            }
        }
    }

    private void ProcessGravity()
    {
        if (_isGrounded && _verticalVelocity < 0)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += gravity * Time.fixedDeltaTime;
    }


    private void UpdateState()
    {
        if (_isDefending)
        {
            SetState(PlayerState.Defense);
        }
        else if (_isDodging)
        {
            SetState(PlayerState.Dodge);
        }
        else if (_isAttacking)
        {
            SetState(PlayerState.Attack);
        }
        else if (!_isGrounded && _verticalVelocity > 0)
        {
            SetState(PlayerState.Jump);
        }
        else if (!_isGrounded && _verticalVelocity < 0)
        {
            SetState(PlayerState.Fall);
        }
        else if (_isRunning && _movement.magnitude > 0.1f)
        {
            SetState(PlayerState.Run);
        }
        else if (_movement.magnitude > 0.1f)
        {
            SetState(PlayerState.Walk);
        }
        else
        {
            SetState(PlayerState.Idle);
        }
    }

    private void SetState(PlayerState newState)
    {
        if (_state == newState)
            return;

        _state = newState;
        animator.SetInteger("state", (int)_state);
    }

    private void ApplyMovement()
    {
        Vector3 finalMovement;

        if (_isDodging)
        {
            finalMovement = _dodgeDirection * dodgeSpeed;
        }else 
            finalMovement = _movement * (_isRunning ? runSpeed : moveSpeed);

        finalMovement.y = _verticalVelocity;
        _controller.Move(finalMovement * Time.fixedDeltaTime);
    }

    private void ComboAttack()
    {

        _isAttacking = true;
        _attackTimer = attackDuration;
        _comboTimer = comboWindow;
        _currentCombo++;

        animator.SetTrigger("attack");
    }

    private void StartDefense()
    {
        if (_isDodging || _isAttacking)
            return;

        _isDefending = true;
        _parryTimer = parryWindow;
        _isParryActive = true;
    }

    private void StopDefense()
    {
        _isDefending = false;
        _isParryActive = false;

    }

} */