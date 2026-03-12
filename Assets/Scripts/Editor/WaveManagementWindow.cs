using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Editor window for managing WaveConfig assets in Resources/Waves.
/// </summary>
public class WaveManagementWindow : EditorWindow
{
    private const string WavesFolderPath = "Assets/Resources/Waves";

    private List<WaveConfig> waveConfigs = new List<WaveConfig>();
    private Vector2 listScrollPos;
    private Vector2 inspectorScrollPos;
    private int selectedIndex = -1;
    private Editor cachedEditor;
    private ReorderableList reorderableList;
    private int pendingDeleteIndex = -1;


    [MenuItem("Tools/Wave Management")]
    public static void ShowWindow()
    {
        WaveManagementWindow window = GetWindow<WaveManagementWindow>("Wave Management");
        window.minSize = new Vector2(700, 400);
        window.LoadAllWaveConfigs();
    }

    private void OnEnable()
    {
        LoadAllWaveConfigs();
    }

    private void OnFocus()
    {
        LoadAllWaveConfigs();
    }

    private void LoadAllWaveConfigs()
    {
        waveConfigs.Clear();

        string[] guids = AssetDatabase.FindAssets("t:WaveConfig", new[] { WavesFolderPath });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            WaveConfig config = AssetDatabase.LoadAssetAtPath<WaveConfig>(path);
            if (config != null)
            {
                waveConfigs.Add(config);
            }
        }

        waveConfigs.Sort((a, b) => a.WaveNumber.CompareTo(b.WaveNumber));

        if (selectedIndex >= waveConfigs.Count)
        {
            selectedIndex = waveConfigs.Count - 1;
        }

        SetupReorderableList();
    }

    private void SetupReorderableList()
    {
        reorderableList = new ReorderableList(waveConfigs, typeof(WaveConfig),
            draggable: true, displayHeader: true, displayAddButton: false, displayRemoveButton: false);

        reorderableList.drawHeaderCallback = (rect) =>
            EditorGUI.LabelField(rect, "Waves  (drag handle to reorder)", EditorStyles.boldLabel);

        reorderableList.elementHeight = 22f;
        reorderableList.drawElementCallback = DrawWaveListElement;
        reorderableList.onReorderCallbackWithDetails = OnWavesReordered;
        reorderableList.onSelectCallback = (list) =>
        {
            selectedIndex = list.index;
            cachedEditor = null;
            Repaint();
        };

        reorderableList.index = Mathf.Clamp(selectedIndex, -1, waveConfigs.Count - 1);
    }

    private void DrawWaveListElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        if (index < 0 || index >= waveConfigs.Count) return;
        WaveConfig config = waveConfigs[index];
        if (config == null) return;

        rect.y += 2f;
        rect.height = EditorGUIUtility.singleLineHeight;

        // Enabled toggle
        Rect toggleRect = new Rect(rect.x, rect.y, 18f, rect.height);
        SerializedObject so = new SerializedObject(config);
        SerializedProperty enabledProp = so.FindProperty("waveEnabled");
        if (enabledProp != null)
        {
            bool newVal = EditorGUI.Toggle(toggleRect, enabledProp.boolValue);
            if (newVal != enabledProp.boolValue)
            {
                enabledProp.boolValue = newVal;
                so.ApplyModifiedProperties();
            }
        }

        // Label
        string label = $"Wave {config.WaveNumber}";
        if (!config.WaveEnabled) label += " (Disabled)";

        float deleteButtonWidth = 20f;
        Rect labelRect = new Rect(rect.x + 22f, rect.y, rect.width - 22f - deleteButtonWidth - 2f, rect.height);
        EditorGUI.LabelField(labelRect, label);

        // Inline delete button
        Color prev = GUI.backgroundColor;
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        Rect deleteRect = new Rect(rect.xMax - deleteButtonWidth, rect.y, deleteButtonWidth, rect.height);
        if (GUI.Button(deleteRect, "X"))
        {
            pendingDeleteIndex = index;
        }
        GUI.backgroundColor = prev;
    }

    private void OnWavesReordered(ReorderableList list, int oldIndex, int newIndex)
    {
        // Reassign wave numbers 1..N based on the new visual order
        for (int i = 0; i < waveConfigs.Count; i++)
        {
            if (waveConfigs[i] == null) continue;
            SerializedObject so = new SerializedObject(waveConfigs[i]);
            so.FindProperty("waveNumber").intValue = i + 1;
            so.ApplyModifiedProperties();
        }

        AssetDatabase.SaveAssets();
        selectedIndex = list.index;
        cachedEditor = null;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // Left panel - wave list
        DrawWaveList();

        // Right panel - inspector
        DrawInspectorPanel();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawWaveList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(260));

        EditorGUILayout.LabelField("Wave Configs", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Found: {waveConfigs.Count} waves", EditorStyles.miniLabel);
        EditorGUILayout.Space(4);

        // Refresh button
        if (GUILayout.Button("Refresh List"))
        {
            LoadAllWaveConfigs();
        }

        EditorGUILayout.Space(4);

        // Check for duplicate wave numbers
        HashSet<int> seenNumbers = new HashSet<int>();
        HashSet<int> duplicateNumbers = new HashSet<int>();
        for (int i = 0; i < waveConfigs.Count; i++)
        {
            if (waveConfigs[i] != null && !seenNumbers.Add(waveConfigs[i].WaveNumber))
            {
                duplicateNumbers.Add(waveConfigs[i].WaveNumber);
            }
        }

        if (duplicateNumbers.Count > 0)
        {
            string nums = string.Join(", ", duplicateNumbers);
            EditorGUILayout.HelpBox($"Duplicate wave numbers detected: {nums}\nDrag to reorder to auto-fix.", MessageType.Warning);
            EditorGUILayout.Space(2);
        }

        // Sync selection into list if changed externally
        if (reorderableList != null && reorderableList.index != selectedIndex)
        {
            reorderableList.index = Mathf.Clamp(selectedIndex, -1, waveConfigs.Count - 1);
        }

        // Scrollable reorderable list
        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos, GUILayout.ExpandHeight(true));

        if (reorderableList != null)
        {
            reorderableList.DoLayoutList();
        }

        // Process pending deletion after list rendering is complete
        if (pendingDeleteIndex >= 0)
        {
            int toDelete = pendingDeleteIndex;
            pendingDeleteIndex = -1;
            DeleteWaveAtIndex(toDelete);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);

        // Create new wave section
        DrawCreateWaveSection();

        EditorGUILayout.EndVertical();
    }

    private int GetNextWaveNumber()
    {
        int max = 0;
        for (int i = 0; i < waveConfigs.Count; i++)
        {
            if (waveConfigs[i] != null && waveConfigs[i].WaveNumber > max)
            {
                max = waveConfigs[i].WaveNumber;
            }
        }
        return max + 1;
    }

    private void DrawCreateWaveSection()
    {
        EditorGUILayout.LabelField("Create New Wave", EditorStyles.boldLabel);

        int nextNumber = GetNextWaveNumber();
        EditorGUILayout.LabelField("Wave Number", nextNumber.ToString());

        if (GUILayout.Button("Create Wave Config"))
        {
            CreateNewWaveConfig();
        }
    }

    private void DrawInspectorPanel()
    {
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));

        if (selectedIndex >= 0 && selectedIndex < waveConfigs.Count)
        {
            WaveConfig selected = waveConfigs[selectedIndex];
            EditorGUILayout.LabelField($"Editing: Wave {selected.WaveNumber}", EditorStyles.boldLabel);

            string assetPath = AssetDatabase.GetAssetPath(selected);
            EditorGUILayout.LabelField(assetPath, EditorStyles.miniLabel);
            EditorGUILayout.Space(4);

            inspectorScrollPos = EditorGUILayout.BeginScrollView(inspectorScrollPos);

            if (cachedEditor == null || cachedEditor.target != selected)
            {
                if (cachedEditor != null)
                {
                    DestroyImmediate(cachedEditor);
                }
                cachedEditor = Editor.CreateEditor(selected);
            }

            if (cachedEditor != null)
            {
                cachedEditor.OnInspectorGUI();
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.LabelField("Select a wave config to edit.", EditorStyles.centeredGreyMiniLabel);
        }

        EditorGUILayout.EndVertical();
    }

    private void CreateNewWaveConfig()
    {
        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(WavesFolderPath))
        {
            string parent = Path.GetDirectoryName(WavesFolderPath).Replace("\\", "/");
            string folderName = Path.GetFileName(WavesFolderPath);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        WaveConfig newConfig = CreateInstance<WaveConfig>();

        int nextNumber = GetNextWaveNumber();

        // Set fields via SerializedObject
        SerializedObject so = new SerializedObject(newConfig);
        so.FindProperty("waveNumber").intValue = nextNumber;
        so.FindProperty("waveEnabled").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Generate unique asset path using timestamp to avoid name/number confusion
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"WaveConfig_{timestamp}.asset";
        string assetPath = $"{WavesFolderPath}/{fileName}";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        AssetDatabase.CreateAsset(newConfig, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        LoadAllWaveConfigs();

        // Select the newly created wave
        for (int i = 0; i < waveConfigs.Count; i++)
        {
            if (waveConfigs[i] == newConfig)
            {
                selectedIndex = i;
                cachedEditor = null;
                break;
            }
        }

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newConfig;
    }

    private void DeleteWaveAtIndex(int index)
    {
        if (index < 0 || index >= waveConfigs.Count) return;

        WaveConfig config = waveConfigs[index];
        string assetPath = AssetDatabase.GetAssetPath(config);

        if (!EditorUtility.DisplayDialog(
            "Delete Wave Config",
            $"Are you sure you want to delete 'Wave {config.WaveNumber}'?\n\nPath: {assetPath}",
            "Delete",
            "Cancel"))
        {
            return;
        }

        if (cachedEditor != null)
        {
            DestroyImmediate(cachedEditor);
            cachedEditor = null;
        }

        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.Refresh();

        selectedIndex = -1;
        LoadAllWaveConfigs();
    }

    private void DeleteSelectedWave()
    {
        if (selectedIndex < 0 || selectedIndex >= waveConfigs.Count) return;

        WaveConfig config = waveConfigs[selectedIndex];
        string assetPath = AssetDatabase.GetAssetPath(config);

        if (!EditorUtility.DisplayDialog(
            "Delete Wave Config",
            $"Are you sure you want to delete 'Wave {config.WaveNumber}'?\n\nPath: {assetPath}",
            "Delete",
            "Cancel"))
        {
            return;
        }

        if (cachedEditor != null)
        {
            DestroyImmediate(cachedEditor);
            cachedEditor = null;
        }

        AssetDatabase.DeleteAsset(assetPath);
        AssetDatabase.Refresh();

        selectedIndex = -1;
        LoadAllWaveConfigs();
    }

    private void OnDestroy()
    {
        if (cachedEditor != null)
        {
            DestroyImmediate(cachedEditor);
        }
    }
}
