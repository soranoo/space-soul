using TMPro;
using UnityEngine;

/// <summary>
/// Visual row for a single credit entry.
/// </summary>
public class CreditLineUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text nameText;

    public void Bind(CreditsItem item)
    {
        if (item == null)
        {
            return;
        }

        if (roleText != null)
        {
            roleText.text = item.Role;
        }

        if (nameText != null)
        {
            nameText.text = item.Name;
        }
    }
}