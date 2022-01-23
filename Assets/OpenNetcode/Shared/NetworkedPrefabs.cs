using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPS/Networked Prefabs")]
public class NetworkedPrefabs : ScriptableObject
{
    [Serializable]
    public struct Pair
    {
        public GameObject Client;
        public GameObject Server;
    }
    
    public List<Pair> Prefabs;
}
