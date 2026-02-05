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
    private ICommand fireCommand;

    private Vector2 moveInput;
    private Vector2 aimPosition;
    private bool isFiring;

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
    public void Initialize(PlayerController player, Rigidbody2D rigidbody, PlayerStats stats, WeaponController weapon)
    {
        this.player = player;

        moveForwardCommand = new MoveForwardCommand(rigidbody, stats);
        rotateAimCommand = new RotateAimCommand(player.transform, stats);
        fireCommand = new FireWeaponCommand(weapon);
    }

    /// <summary>
    /// Swap the fire command (for upgrades).
    /// </summary>
    public void SetFireCommand(ICommand command)
    {
        fireCommand = command;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void Update()
    {
        HandleFiring();
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

    private void HandleFiring()
    {
        if (isFiring)
        {
            fireCommand?.Execute();
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
    }

    private void OnAttackCanceled(InputAction.CallbackContext context)
    {
        isFiring = false;
    }

    private void OnPointPerformed(InputAction.CallbackContext context)
    {
        aimPosition = context.ReadValue<Vector2>();
    }
}
