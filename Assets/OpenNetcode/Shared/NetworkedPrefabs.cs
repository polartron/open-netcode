using System;
using System.Collections;
using System.Collections.Generic;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "RPS/Networked Prefabs")]
public class NetworkedPrefabs : ScriptableObject
{
    [SerializeField] private List<GameObject> _prefabs;

    public List<GameObject> Prefabs
    {
        get
        {
#if UNITY_EDITOR
            List<GameObject> prefabs = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (go.GetComponent<NetworkedPrefabBehaviour>() != null)
                {
                    prefabs.Add(go);
                }
            }

            return prefabs;
#else
            return _prefabs;
#endif
        }
    }
}