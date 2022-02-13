using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveDataImplementation
{
    public void Save();
    public void Load();
}

public class SaveData
{
    internal static ISaveDataImplementation Implementation;
    
    public static void Set<T>(string key, T value)
    {
        
    }
}
