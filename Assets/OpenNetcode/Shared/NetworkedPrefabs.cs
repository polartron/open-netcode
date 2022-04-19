using System.Collections.Generic;
using OpenNetcode.Shared.Authoring;
using UnityEditor;
using UnityEngine;

namespace OpenNetcode.Shared
{
    [CreateAssetMenu(menuName = "RPS/Networked Prefabs")]
    public class NetworkedPrefabs : ScriptableObject
    {
        public GameObject ClientPlayer;
        
        [SerializeField] [HideInInspector] private List<GameObject> _serverPrefabs;
        [SerializeField] [HideInInspector] private List<GameObject> _clientPrefabs;

        public List<GameObject> Server
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
                
                    if (go.GetComponent<ServerPrefabAuthoring>() != null)
                    {
                        prefabs.Add(go);
                    }
                }

                return prefabs;
#else
                return _serverPrefabs;
#endif
            }

            set
            {
                _serverPrefabs = value;
            }
        }
        
        public List<GameObject> Client
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
                
                    if (go.GetComponent<ClientPrefabAuthoring>() != null)
                    {
                        prefabs.Add(go);
                    }
                }

                return prefabs;
#else
                return _clientPrefabs;
#endif
            }

            set
            {
                _clientPrefabs = value;
            }
        }
    }
}