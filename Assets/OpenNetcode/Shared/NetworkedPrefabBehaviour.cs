using System;
using OpenNetcode.Shared.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace OpenNetcode.Shared
{
    public class NetworkedPrefabBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    {
        [HideInInspector] [SerializeField] private string GUID;
        public string Guid => GUID;

        void OnValidate()
        {
            if (string.IsNullOrEmpty(GUID))
            {
                GUID = System.Guid.NewGuid().ToString();
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<NetworkedPrefabGuid>(entity);
            
            dstManager.SetComponentData(entity, new NetworkedPrefabGuid()
            {
                Value = new FixedString64Bytes(GUID)
            });
        }
    }
}
