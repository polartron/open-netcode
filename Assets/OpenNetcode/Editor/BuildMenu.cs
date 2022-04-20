using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using OpenNetcode.Shared;
using UnityEditor;
using UnityEngine;

public class BuildMenu
{
    [MenuItem("OpenNetcode/Build/Build Client")]
    public static void BuildClient()
    {
        BuildPlayerOptions defaultOptions = new BuildPlayerOptions();
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        defaultOptions.target = EditorUserBuildSettings.activeBuildTarget;
        defaultOptions.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        defaultOptions.options = BuildOptions.Development;
        defaultOptions.scenes = new[]
        {
            "Assets/Scenes/ClientScene.unity"
        };
        
        defaultOptions.extraScriptingDefines = new[]
        {
            "CLIENT"
        };
        defaultOptions.locationPathName = "Build/Client.exe";
        
        NetworkedPrefabs networkedPrefabs = Resources.Load<NetworkedPrefabs>("Networked Prefabs");
        var prefabs = networkedPrefabs.Client;
        networkedPrefabs.Client = prefabs;
        networkedPrefabs.Server = null;
        Debug.Log($"Saving {prefabs.Count} client prefabs.");
        EditorUtility.SetDirty(networkedPrefabs);
        AssetDatabase.SaveAssets();
        
        BuildPipeline.BuildPlayer(defaultOptions);
    }
    
    [MenuItem("OpenNetcode/Build/Build Client Scripts Only")]
    public static void BuildClientScriptsOnly()
    {
        BuildPlayerOptions defaultOptions = new BuildPlayerOptions();
        EditorUserBuildSettings.SetPlatformSettings("Standalone", "CopyPDBFiles", "true");
        defaultOptions.target = EditorUserBuildSettings.activeBuildTarget;
        defaultOptions.targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
        defaultOptions.options = BuildOptions.Development | BuildOptions.BuildScriptsOnly;
        defaultOptions.scenes = new[]
        {
            "Assets/Scenes/ClientScene.unity"
        };
        
        defaultOptions.extraScriptingDefines = new[]
        {
            "CLIENT"
        };
        defaultOptions.locationPathName = "Build/Client.exe";

        NetworkedPrefabs networkedPrefabs = Resources.Load<NetworkedPrefabs>("Networked Prefabs");
        var prefabs = networkedPrefabs.Client;
        networkedPrefabs.Client = prefabs;
        networkedPrefabs.Server = null;
        Debug.Log($"Saving {prefabs.Count} client prefabs.");
        EditorUtility.SetDirty(networkedPrefabs);
        AssetDatabase.SaveAssets();
        
        BuildPipeline.BuildPlayer(defaultOptions);
    }
    
    static BuildPlayerOptions GetBuildPlayerOptions(
        bool askForLocation = false,
        BuildPlayerOptions defaultOptions = new BuildPlayerOptions())
    {
        // Get static internal "GetBuildPlayerOptionsInternal" method
        MethodInfo method = typeof(BuildPlayerWindow).GetMethod(
            "GetBuildPlayerOptionsInternal",
            BindingFlags.NonPublic | BindingFlags.Static);
     
        // invoke internal method
        return (BuildPlayerOptions) method.Invoke(
            null, 
            new object[] { askForLocation, defaultOptions});
    }
}
