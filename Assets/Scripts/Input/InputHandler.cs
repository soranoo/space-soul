using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Maps input to commands and executes them using Unity Input System.
/// </summary>
public class InputHandler : MonoBehaviour
{
    private PlayerController player;
    private InputSystem_Actions inputActions;
    private GameManager gameManager;

    private ICommand moveForwardCommand;
    private RotateAimCommand rotateAimCommand;
    private Vector2 moveInput;
    private Vector2 aimPosition;
    private bool isFiring;
    private bool allowFire;
    private bool allowGameplayInput;

    /// <summary>
    /// Whether the player is currently applying thrust input.
    /// </summary>
    public bool IsThrusting => moveInput.y > 0f;

    /// <summary>
    /// Current movement input vector.
    /// </summary>
    public Vector2 MoveInput => moveInput;

    /// <summary>
    /// Whether the player is currently holding the fire input.
    /// </summary>
    public bool IsFiring => isFiring;

    /// <summary>
    /// Fired when the fire input starts.
    /// </summary>
    public event Action FireStarted;

    /// <summary>
    /// Fired when the fire input stops.
    /// </summary>
    public event Action FireStopped;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        gameManager = GameManager.Instance;
    }

    private void OnEnable()
    {
        gameManager.StateMachine.StateChanged += OnStateChanged;
        UpdateInputAllowed(gameManager.StateMachine.CurrentState);


        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Attack.canceled += OnAttackCanceled;
        inputActions.Player.Point.performed += OnPointPerformed;

        ApplyPlayerActionMapState();
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Attack.canceled -= OnAttackCanceled;
        inputActions.Player.Point.performed -= OnPointPerformed;

        gameManager.StateMachine.StateChanged -= OnStateChanged;

        moveInput = Vector2.zero;
        isFiring = false;

        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }

    /// <summary>
    /// Initialize commands with player references.
    /// </summary>
    public void Initialize(PlayerController player, Rigidbody2D rigidbody, PlayerStats stats)
    {
        this.player = player;

        moveForwardCommand = new MoveForwardCommand(rigidbody, stats);
        rotateAimCommand = new RotateAimCommand(player.transform, stats);
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }


    private void HandleMovement()
    {
        if (!allowGameplayInput)
        {
            return;
        }

        // Thrust forward when pressing up (y > 0).
        if (moveInput.y > 0f)
        {
            moveForwardCommand?.Execute();
        }

        // Rotate player to face the mouse cursor.
        if (rotateAimCommand != null)
        {
            rotateAimCommand.SetAimPosition(aimPosition);
            rotateAimCommand.Execute();
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!allowGameplayInput)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        if (!allowFire)
        {
            return;
        }

        isFiring = true;
        FireStarted?.Invoke();
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        if (!allowFire)
        {
            return;
        }

        isFiring = false;
        FireStopped?.Invoke();
    }

    private void OnPointPerformed(InputAction.CallbackContext context)
    {
        if (!allowGameplayInput)
        {
            return;
        }

        aimPosition = context.ReadValue<Vector2>();
    }

    private void OnStateChanged(IGameState previousState, IGameState currentState)
    {
        UpdateInputAllowed(currentState);
    }

    private void UpdateInputAllowed(IGameState currentState)
    {
        allowGameplayInput = currentState is GameplayState;
        allowFire = allowGameplayInput;

        if (!allowGameplayInput)
        {
            moveInput = Vector2.zero;

            if (!isFiring)
            {
                ApplyPlayerActionMapState();
                return;
            }

            isFiring = false;
            FireStopped?.Invoke();
        }

        ApplyPlayerActionMapState();
    }

    private void ApplyPlayerActionMapState()
    {
        if (allowGameplayInput)
        {
            inputActions.Player.Enable();
            return;
        }

        inputActions.Player.Disable();
    }
}
