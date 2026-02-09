using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// Manages the upgrade selection screen shown between waves.
/// Populates UpgradeCard instances, handles selection, and notifies GameManager to proceed.
/// </summary>
public class UpgradeSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform cardsParent;
    [SerializeField] private UpgradeCard cardPrefab;

    [Header("Card Animation")]
    [SerializeField] private float staggerDelay = 0.12f;

    private readonly List<UpgradeCard> activeCards = new List<UpgradeCard>();

    /// <summary>
    /// Fired after an upgrade has been selected and applied.
    /// </summary>
    public event Action UpgradeSelected;

    /// <summary>
    /// Show the upgrade selection panel with randomly chosen upgrades.
    /// </summary>
    public void Show()
    {
        ClearCards();

        if (UpgradeManager.Instance == null)
        {
            Debug.LogWarning("UpgradeSelectionUI: UpgradeManager not found.");
            return;
        }

        List<UpgradeData> upgrades = UpgradeManager.Instance.GetRandomUpgrades();

        if (upgrades.Count == 0)
        {
            // No valid upgrades — skip the selection entirely
            UpgradeSelected?.Invoke();
            return;
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        for (int i = 0; i < upgrades.Count; i++)
        {
            UpgradeCard card = Instantiate(cardPrefab, cardsParent);
            card.Setup(upgrades[i], OnCardSelected);
            card.popDelay = staggerDelay * i;
            card.PlayPopIn();
            activeCards.Add(card);
        }

        RefreshLayout();
    }

    /// <summary>
    /// Hide the upgrade selection panel.
    /// </summary>
    public void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        ClearCards();
    }

    private void OnCardSelected(UpgradeData selected)
    {
        // Disable all buttons to prevent double-select
        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].GetComponentInChildren<UnityEngine.UI.Button>().interactable = false;
        }

        UpgradeManager.Instance?.ApplyUpgrade(selected);

        // Pop out all cards then fire event
        int remaining = activeCards.Count;

        for (int i = 0; i < activeCards.Count; i++)
        {
            activeCards[i].PlayPopOut(() =>
            {
                remaining--;
                if (remaining <= 0)
                {
                    Hide();
                    UpgradeSelected?.Invoke();
                }
            });
        }
    }

    private void ClearCards()
    {
        for (int i = 0; i < activeCards.Count; i++)
        {
            if (activeCards[i] != null)
            {
                Destroy(activeCards[i].gameObject);
            }
        }

        activeCards.Clear();
    }

    private void RefreshLayout()
    {
        if (cardsParent == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(cardsParent);
    }
}
