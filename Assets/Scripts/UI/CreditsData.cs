using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject data source for main menu credits.
/// </summary>
[CreateAssetMenu(fileName = "CreditsData", menuName = "Game/UI/Credits Data")]
public class CreditsData : ScriptableObject
{
    [SerializeField] private List<CreditsSection> sections = new List<CreditsSection>();

    public IReadOnlyList<CreditsSection> Sections => sections;
}

[Serializable]
public class CreditsSection
{
    [SerializeField] private string title = "Section";
    [SerializeField] private List<CreditsItem> items = new List<CreditsItem>();

    public string Title => title;
    public IReadOnlyList<CreditsItem> Items => items;
}

[Serializable]
public class CreditsItem
{
    [SerializeField] private string role = "Role";
    [SerializeField] private string name = "Name";

    public string Role => role;
    public string Name => name;
}