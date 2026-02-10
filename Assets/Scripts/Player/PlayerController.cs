using UnityEngine;
using System;

/// <summary>
/// Main player ship behavior.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(PowerUpController))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats = new PlayerStats();

    private PowerUpController powerUpController;
    private InputHandler inputHandler;
    private Rigidbody2D rb;
    private int currentHealth;
    private float regenAccumulator;

    /// <summary>
    /// Current health points.
    /// </summary>
    public int CurrentHealth => currentHealth;

    /// <summary>
    /// Player stats reference.
    /// </summary>
    public PlayerStats Stats => stats;

    /// <summary>
    /// Input handler reference.
    /// </summary>
    public InputHandler InputHandler => inputHandler;

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
        inputHandler = GetComponent<InputHandler>();

        if (powerUpController == null)
        {
            powerUpController = GetComponent<PowerUpController>();
        }
    }

    private void Start()
    {
        currentHealth = stats.MaxHealth;

        HealthChanged?.Invoke(currentHealth, stats.MaxHealth);

        inputHandler?.Initialize(this, rb, stats);
    }

    private void FixedUpdate()
    {
        ClampVelocity();
    }

    private void Update()
    {
        TickHealthRegen();
    }

    private void TickHealthRegen()
    {
        if (stats.HealthRegenRate <= 0f || currentHealth >= stats.MaxHealth)
        {
            return;
        }

        regenAccumulator += stats.HealthRegenRate * Time.deltaTime;

        if (regenAccumulator >= 1f)
        {
            int points = Mathf.FloorToInt(regenAccumulator);
            regenAccumulator -= points;
            Heal(points);
        }
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

        if (powerUpController != null)
        {
            amount = powerUpController.ProcessIncomingDamage(amount);
            if (amount <= 0)
            {
                return;
            }
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
