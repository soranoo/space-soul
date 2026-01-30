using UnityEngine;
using System;

/// <summary>
/// Main player ship behavior.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats = new PlayerStats();

    [Header("Components")]
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private InputHandler inputHandler;

    private Rigidbody2D rb;
    private int currentHealth;

    /// <summary>
    /// Current health points.
    /// </summary>
    public int CurrentHealth => currentHealth;

    /// <summary>
    /// Player stats reference.
    /// </summary>
    public PlayerStats Stats => stats;

    /// <summary>
    /// Event fired when health changes.
    /// </summary>
    public event Action<int, int> HealthChanged;

    /// <summary>
    /// Event fired when player dies.
    /// </summary>
    public event Action Died;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (weaponController == null)
        {
            weaponController = GetComponentInChildren<WeaponController>();
        }

        if (inputHandler == null)
        {
            inputHandler = GetComponent<InputHandler>();
        }
    }

    private void Start()
    {
        currentHealth = stats.MaxHealth;

        HealthChanged?.Invoke(currentHealth, stats.MaxHealth);

        weaponController?.Initialize(stats);
        inputHandler?.Initialize(this, rb, stats, weaponController);
    }

    private void FixedUpdate()
    {
        ClampVelocity();
    }

    private void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > stats.MaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * stats.MaxSpeed;
        }
    }

    /// <summary>
    /// Apply damage to the player.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth -= amount;
        HealthChanged?.Invoke(currentHealth, stats.MaxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal the player.
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, stats.MaxHealth);
        HealthChanged?.Invoke(currentHealth, stats.MaxHealth);
    }

    private void Die()
    {
        Died?.Invoke();
        GameManager.Instance?.GameOver();
    }
}
