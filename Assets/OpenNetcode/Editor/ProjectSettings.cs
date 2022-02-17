using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEngine;

namespace OpenNetcode.Editor
{
    [Serializable]
    public class Paths
    {
        public string Client = "<YOUR GAME HERE>/Client/Generated/";
        public string Server = "<YOUR GAME HERE>/Server/Generated/";
        public string Shared = "<YOUR GAME HERE>/Shared/Generated/";
    }

    public enum TicksEnum
    {
        TICKRATE_128,
        TICKRATE_64,
        TICKRATE_32,
        TICKRATE_16,
        TICKRATE_8,
    }

    public enum SnapshotsEnum
    {
        SNAPSHOTRATE_128,
        SNAPSHOTRATE_64,
        SNAPSHOTRATE_32,
        SNAPSHOTRATE_16,
        SNAPSHOTRATE_8,
        SNAPSHOTRATE_4,
        SNAPSHOTRATE_2
    }

    [Serializable]
    public class TickSettings
    {
        public TicksEnum TicksPerSecond = TicksEnum.TICKRATE_64;
        public SnapshotsEnum SnapshotsPerSecond = SnapshotsEnum.SNAPSHOTRATE_16;
    }

    [CreateAssetMenu(menuName = "OpenNetcode/Project Settings")]
    public class ProjectSettings : ScriptableObject
    {
        public Paths CodeGenerationPaths = new Paths();
        public TickSettings TickSettings = new TickSettings();
        public void ApplySettings()
        {
            List<string> defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';').ToList();

            if (defines.Contains(TickSettings.TicksPerSecond.ToString()) &&
                defines.Contains(TickSettings.SnapshotsPerSecond.ToString()))
                return;
            
            foreach (var name in Enum.GetNames(typeof(TicksEnum)))
            {
                defines.Remove(name);
            }
            
            foreach (var name in Enum.GetNames(typeof(SnapshotsEnum)))
            {
                defines.Remove(name);
            }
            
            defines.Add(TickSettings.TicksPerSecond.ToString());
            defines.Add(TickSettings.SnapshotsPerSecond.ToString());

            string joined = string.Join(";", defines.ToArray());

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, joined);
        }
    }
}