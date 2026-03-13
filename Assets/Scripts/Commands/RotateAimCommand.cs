using UnityEngine;

/// <summary>
/// Command to rotate the player to face the mouse cursor position.
/// Receives aim position from InputHandler via Input System bindings.
/// </summary>
public class RotateAimCommand : ICommand
{
    private readonly Transform transform;
    private readonly PlayerController player;
    private readonly Camera mainCamera;

    private Vector2 aimScreenPosition;

    public RotateAimCommand(Transform transform, PlayerController player)
    {
        this.transform = transform;
        this.player = player;
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Update the aim position from Input System binding.
    /// </summary>
    public void SetAimPosition(Vector2 screenPosition)
    {
        aimScreenPosition = screenPosition;
    }

    public bool CanExecute()
    {
        return transform != null && mainCamera != null;
    }

    public void Execute()
    {
        if (!CanExecute())
        {
            return;
        }

        // Convert screen position to world space
        Vector3 mouseScreenPos = new Vector3(aimScreenPosition.x, aimScreenPosition.y, mainCamera.nearClipPlane);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // Calculate direction from player to mouse
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        // Calculate target angle (sprite faces up, so we use Atan2 with y, x adjustment)
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // Smoothly rotate towards target angle
        float currentAngle = transform.eulerAngles.z;
        float rotationSpeed = player != null ? player.GetCurrentRotationSpeed() : 0f;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }
}
