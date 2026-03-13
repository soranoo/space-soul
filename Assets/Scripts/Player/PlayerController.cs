using UnityEngine;
using System;

/// <summary>
/// Main player ship behavior.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PowerUpController))]
public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats = new PlayerStats();

    [Header("Compoents")]
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private PlayerEngineController engineController;

    private PowerUpController powerUpController;
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
        powerUpController = GetComponent<PowerUpController>();

        if (engineController == null)
        {
            engineController = GetComponentInChildren<PlayerEngineController>();
        }

        if (powerUpController != null)
        {
            inputHandler = GetComponent<InputHandler>();
        }
    }

    private void Start()
    {
        currentHealth = stats.MaxHealth;

        HealthChanged?.Invoke(currentHealth, stats.MaxHealth);

        inputHandler.Initialize(this, rb);
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
        float maxSpeed = GetCurrentMaxSpeed();
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    public float GetCurrentThrustForce()
    {
        PlayerEngine engine = engineController != null ? engineController.CurrentEngine : null;
        if (engine == null)
        {
            return 0f;
        }

        return engine.GetThrustForce(stats);
    }

    public float GetCurrentRotationSpeed()
    {
        PlayerEngine engine = engineController != null ? engineController.CurrentEngine : null;
        if (engine == null)
        {
            return 0f;
        }

        return engine.GetRotationSpeed(stats);
    }

    public float GetCurrentMaxSpeed()
    {
        PlayerEngine engine = engineController != null ? engineController.CurrentEngine : null;
        if (engine == null)
        {
            return 0f;
        }

        return engine.GetMaxSpeed(stats);
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

        currentHealth = Math.Max(currentHealth - amount, 0);
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
