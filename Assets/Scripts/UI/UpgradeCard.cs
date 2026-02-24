using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

/// <summary>
/// UI component for a single upgrade card.
/// Expects hierarchy: Card > Top/Text-Title, Body/Image-Icon/Text-Description, Bottom/Button-Select.
/// </summary>
public class UpgradeCard : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button selectButton;

    [Header("Animation")]
    [SerializeField] public float popDelay = 0f;
    [SerializeField] private float popDuration = 0.35f;
    [SerializeField] private Ease popEase = Ease.OutBack;

    private UpgradeData data;
    private Action<UpgradeData> onSelected;
    private RectTransform rectTransform;

    /// <summary>
    /// The upgrade data this card is currently displaying.
    /// </summary>
    public UpgradeData Data => data;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectClicked);
        }
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnSelectClicked);
        }
    }

    /// <summary>
    /// Configure the card with upgrade data and a selection callback.
    /// </summary>
    public void Setup(UpgradeData upgradeData, Action<UpgradeData> selectionCallback)
    {
        data = upgradeData;
        onSelected = selectionCallback;

        if (titleText != null)
        {
            titleText.text = data.Title;
        }

        if (descriptionText != null)
        {
            descriptionText.text = data.Description;
        }

        if (iconImage != null)
        {
            if (data.Icon != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
    }

    /// <summary>
    /// Play pop-in entrance animation.
    /// </summary>
    public void PlayPopIn()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.localScale = Vector3.zero;
        rectTransform.DOScale(Vector3.one, popDuration)
            .SetDelay(popDelay)
            .SetEase(popEase)
            .SetUpdate(true);
    }

    /// <summary>
    /// Play pop-out exit animation with callback.
    /// </summary>
    public void PlayPopOut(Action onComplete)
    {
        if (rectTransform == null)
        {
            onComplete?.Invoke();
            return;
        }

        rectTransform.DOScale(Vector3.zero, popDuration * 0.6f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void OnSelectClicked()
    {
        UIManager.Instance.OnButtonClick();
        onSelected?.Invoke(data);
    }
}
