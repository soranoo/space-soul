using TMPro;
using UnityEngine;

/// <summary>
/// Visual block for one credits section.
/// </summary>
public class CreditsSectionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform linesRoot;
    [SerializeField] private CreditLineUI creditLinePrefab;

    public Transform LinesRoot => linesRoot;
    public CreditLineUI CreditLinePrefab => creditLinePrefab;

    public void Bind(CreditsSection section)
    {
        if (titleText != null)
        {
            titleText.text = section != null ? section.Title : string.Empty;
        }
    }
}