using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles options menu navigation and submenu back-stack behavior.
/// Supports opening credits from options and returning to previous menus.
/// </summary>
public class OptionsMenuUI : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject optionsMenuRoot;
    [SerializeField] private CreditsPanelUI creditsPanel;

    private readonly Stack<GameObject> menuHistory = new Stack<GameObject>();
    private GameObject currentMenu;

    private void OnEnable()
    {
        ResetMenuState();
    }

    /// <summary>
    /// Opens the options menu from the current menu.
    /// </summary>
    public void OnOpenOptionsPressed()
    {
        UIManager.Instance.OnButtonClick();
        OpenMenu(optionsMenuRoot);
    }

    /// <summary>
    /// Opens credits panel from the options menu.
    /// </summary>
    public void OnOpenCreditsPressed()
    {
        UIManager.Instance.OnButtonClick();

        GameObject creditsRoot = creditsPanel != null ? creditsPanel.gameObject : null;
        OpenMenu(creditsRoot);

        creditsPanel?.Show();
    }

    /// <summary>
    /// Goes back to the previous menu in history.
    /// </summary>
    public void OnBackPressed()
    {
        UIManager.Instance.OnButtonClick();
        GoBack();
    }

    private void ResetMenuState()
    {
        menuHistory.Clear();

        SetMenuActive(optionsMenuRoot, false);
        creditsPanel?.Hide();
    }

    private void OpenMenu(GameObject nextMenu)
    {
        if (nextMenu == null || nextMenu == currentMenu)
        {
            return;
        }

        if (currentMenu != null)
        {
            menuHistory.Push(currentMenu);
            SetMenuActive(currentMenu, false);
        }

        currentMenu = nextMenu;
        SetMenuActive(currentMenu, true);
    }

    private void GoBack()
    {
        if (menuHistory.Count == 0)
        {
            SetMenuActive(currentMenu, false);
            currentMenu = null;
            return;
        }

        SetMenuActive(currentMenu, false);

        currentMenu = menuHistory.Pop();
        SetMenuActive(currentMenu, true);
    }

    private void SetMenuActive(GameObject menuRoot, bool isActive)
    {
        if (menuRoot == null)
        {
            return;
        }

        if (creditsPanel != null && menuRoot == creditsPanel.gameObject)
        {
            if (isActive)
            {
                creditsPanel.Show();
            }
            else
            {
                creditsPanel.Hide();
            }

            return;
        }

        menuRoot.SetActive(isActive);
    }
}
