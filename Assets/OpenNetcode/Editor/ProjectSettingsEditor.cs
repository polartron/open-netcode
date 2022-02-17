using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenNetcode.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProjectSettings))]
public class ProjectSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (ProjectSettings) target;

        List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();

        if (defines.Contains(script.TickSettings.TicksPerSecond.ToString()) && defines.Contains(script.TickSettings.SnapshotsPerSecond.ToString()))
            return;
        
        if(GUILayout.Button("Apply Tick Settings\n (will trigger full recompile)", GUILayout.Height(40)))
        {
            script.ApplySettings();
        }
    }
}