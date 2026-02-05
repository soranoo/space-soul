using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Maps input to commands and executes them using Unity Input System.
/// </summary>
public class InputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;

    private InputSystem_Actions inputActions;

    private ICommand moveForwardCommand;
    private RotateAimCommand rotateAimCommand;
    private Vector2 moveInput;
    private Vector2 aimPosition;
    private bool isFiring;

    /// <summary>
    /// Whether the player is currently applying thrust input.
    /// </summary>
    public bool IsThrusting => moveInput.y > 0f;

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
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();

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
        isFiring = true;
        FireStarted?.Invoke();
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        isFiring = false;
        FireStopped?.Invoke();
    }

    private void OnPointPerformed(InputAction.CallbackContext context)
    {
        aimPosition = context.ReadValue<Vector2>();
    }
}
