using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles options menu navigation and submenu back-stack behavior.
/// Supports opening credits from options and returning to previous menus.
/// </summary>
public class OptionsMenuUI : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject optionsMenuRoot;
    [SerializeField] private CreditsPanelUI creditsPanel;

    [Header("Options")]
    [SerializeField] private Toggle fpsCounterToggle;
    [SerializeField] private FpsCounterUI fpsCounter;

    private readonly Stack<GameObject> menuHistory = new Stack<GameObject>();
    private GameObject currentMenu;

    private IEnumerator Start()
    {
        // Yield one frame so Unity's audio system is fully initialized before pushing mixer values.
        yield return null;

        AudioVolumeControlUI[] controls = GetComponentsInChildren<AudioVolumeControlUI>(true);
        foreach (AudioVolumeControlUI control in controls)
        {
            control.ApplySavedMixerValue();
        }
    }

    private void OnEnable()
    {
        ResetMenuState();

        if (fpsCounterToggle != null)
        {
            fpsCounterToggle.SetIsOnWithoutNotify(fpsCounter != null && fpsCounter.IsVisible);
            fpsCounterToggle.onValueChanged.AddListener(OnFpsCounterToggleChanged);
        }
    }

    private void OnDisable()
    {
        if (fpsCounterToggle != null)
        {
            fpsCounterToggle.onValueChanged.RemoveListener(OnFpsCounterToggleChanged);
        }
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

    /// <summary>
    /// Toggle callback for showing or hiding FPS counter UI.
    /// </summary>
    public void OnFpsCounterToggleChanged(bool isOn)
    {
        if (fpsCounter == null)
        {
            return;
        }

        UIManager.Instance.OnButtonClick();
        fpsCounter.SetVisible(isOn);
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
