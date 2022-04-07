using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchParameters : MonoBehaviour
{
    // Start is called before the first frame update
    public string EditorLaunchCommands;
    
    void Start()
    {
        #if UNITY_EDITOR
        string args = EditorLaunchCommands;
        #else
        string args = System.Environment.CommandLine;
        #endif
        string[] commands = args.Split('+', '-');
        
        for (int i = 0; i < commands.Length; i++) 
        {
            string commandWithoutPrefix = commands[i];
            commandWithoutPrefix = commandWithoutPrefix.Replace("+", "");
            commandWithoutPrefix = commandWithoutPrefix.Replace("-", "");
            SourceConsole.SourceConsole.ExecuteString(commandWithoutPrefix); 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
