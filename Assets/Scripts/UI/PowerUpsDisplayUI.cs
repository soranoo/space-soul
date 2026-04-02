using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns and manages power-up timer items in the UI.
/// </summary>
public class PowerUpsDisplayUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PowerUpController powerUpController;
    [SerializeField] private PowerUpDisplayItem itemPrefab;
    [SerializeField] private RectTransform container;

    private readonly Dictionary<PowerUpType, PowerUpDisplayItem> items = new Dictionary<PowerUpType, PowerUpDisplayItem>();
    private readonly List<PowerUpController.PowerUpStatus> cachedStatuses = new List<PowerUpController.PowerUpStatus>();

    private void OnEnable()
    {
        if (powerUpController == null)
        {
            powerUpController = FindFirstObjectByType<PowerUpController>();
        }

        Subscribe();
        RefreshExisting();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (powerUpController == null)
        {
            return;
        }

        powerUpController.PowerUpApplied += OnPowerUpApplied;
        powerUpController.PowerUpExpired += OnPowerUpExpired;
    }

    private void Unsubscribe()
    {
        if (powerUpController == null)
        {
            return;
        }

        powerUpController.PowerUpApplied -= OnPowerUpApplied;
        powerUpController.PowerUpExpired -= OnPowerUpExpired;
    }

    private void RefreshExisting()
    {
        if (powerUpController == null)
        {
            return;
        }

        powerUpController.GetActivePowerUps(cachedStatuses);
        for (int i = 0; i < cachedStatuses.Count; i++)
        {
            PowerUpController.PowerUpStatus status = cachedStatuses[i];
            CreateOrRefreshItem(status.Type, status.Data, status.Remaining, status.Duration);
        }
    }

    private void OnPowerUpApplied(PowerUpData data, float duration)
    {
        if (data == null)
        {
            return;
        }

        CreateOrRefreshItem(data.PowerUpType, data, duration, duration);
    }

    private void OnPowerUpExpired(PowerUpType type)
    {
        if (!items.TryGetValue(type, out PowerUpDisplayItem item))
        {
            return;
        }

        if (item != null)
        {
            Destroy(item.gameObject);
        }

        items.Remove(type);
    }

    private void CreateOrRefreshItem(PowerUpType type, PowerUpData data, float remaining, float duration)
    {
        if (itemPrefab == null || container == null)
        {
            return;
        }

        if (!items.TryGetValue(type, out PowerUpDisplayItem item) || item == null)
        {
            item = Instantiate(itemPrefab, container);
            LayoutRebuilder.ForceRebuildLayoutImmediate(container);
            items[type] = item;
        }

        item.Refresh(data, remaining > 0f ? remaining : duration);
    }
}
