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
        inputActions.Player.Enable();
        
        gameManager.StateMachine.StateChanged += OnStateChanged;
        UpdateFireAllowed(gameManager.StateMachine.CurrentState);

        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Attack.performed += OnAttackPerformed;
        inputActions.Player.Attack.canceled += OnAttackCanceled;
        inputActions.Player.Point.performed += OnPointPerformed;
    }

    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        inputActions.Player.Attack.canceled -= OnAttackCanceled;
        inputActions.Player.Point.performed -= OnPointPerformed;

        gameManager.StateMachine.StateChanged -= OnStateChanged;

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
        aimPosition = context.ReadValue<Vector2>();
    }

    private void OnStateChanged(IGameState previousState, IGameState currentState)
    {
        UpdateFireAllowed(currentState);
    }

    private void UpdateFireAllowed(IGameState currentState)
    {
        allowFire = currentState is GameplayState;

        if (!allowFire && isFiring)
        {
            isFiring = false;
            FireStopped?.Invoke();
        }
    }
}
