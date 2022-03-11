using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
            "Assets/Scenes/Test.unity"
        };
        
        defaultOptions.extraScriptingDefines = new[]
        {
            "CLIENT"
        };
        defaultOptions.locationPathName = "Build/Client.exe";
        
        
        
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
