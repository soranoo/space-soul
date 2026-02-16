using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Renders credits data into a UI container.
/// </summary>
public class CreditsPanelUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private CreditsData creditsData;

    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private CreditsSectionUI sectionPrefab;

    private readonly List<GameObject> spawnedEntries = new List<GameObject>();

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }
    }

    private void OnEnable()
    {
        Render();
    }

    public void Show()
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        Render();
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    public void Render()
    {
        if (contentRoot == null || sectionPrefab == null)
        {
            return;
        }

        ClearSpawnedEntries();

        if (creditsData == null)
        {
            return;
        }

        IReadOnlyList<CreditsSection> sections = creditsData.Sections;
        for (int sectionIndex = 0; sectionIndex < sections.Count; sectionIndex++)
        {
            CreditsSection section = sections[sectionIndex];
            if (section == null)
            {
                continue;
            }

            CreditsSectionUI sectionView = Instantiate(sectionPrefab, contentRoot);
            sectionView.Bind(section);
            spawnedEntries.Add(sectionView.gameObject);

            Transform linesRoot = sectionView.LinesRoot != null ? sectionView.LinesRoot : sectionView.transform;
            CreditLineUI linePrefab = sectionView.CreditLinePrefab;
            if (linePrefab == null)
            {
                continue;
            }

            IReadOnlyList<CreditsItem> items = section.Items;
            for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
            {
                CreditsItem item = items[itemIndex];
                if (item == null)
                {
                    continue;
                }

                CreditLineUI lineView = Instantiate(linePrefab, linesRoot);
                lineView.Bind(item);
            }
        }

        RefreshLayout();
    }

    private void ClearSpawnedEntries()
    {
        for (int i = 0; i < spawnedEntries.Count; i++)
        {
            GameObject entry = spawnedEntries[i];
            if (entry != null)
            {
                Destroy(entry);
            }
        }

        spawnedEntries.Clear();
    }

    private void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (contentRoot is RectTransform contentRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        if (root != null && root.transform is RectTransform rootRect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
        }

        Canvas.ForceUpdateCanvases();
    }
}