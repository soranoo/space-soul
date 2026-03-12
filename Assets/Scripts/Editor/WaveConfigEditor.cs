using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom inspector for WaveConfig that shows an "Open Wave Management" button
/// when viewed outside the Wave Management window.
/// </summary>
[CustomEditor(typeof(WaveConfig))]
public class WaveConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Only show the banner when NOT being drawn inside WaveManagementWindow
        if (EditorWindow.focusedWindow == null ||
            !(EditorWindow.focusedWindow is WaveManagementWindow))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Wave Management", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Edit this asset through the Wave Management window for full control.", EditorStyles.wordWrappedMiniLabel);
            if (GUILayout.Button("Open Wave Management"))
            {
                WaveManagementWindow.ShowWindow();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(6);
        }

        DrawDefaultInspector();
    }
}
