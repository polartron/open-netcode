using System.Collections.Generic;
using OpenNetcode.Shared.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace OpenNetcode.Shared.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class NetworkedPrefabSystem : SystemBase
    {
        public NativeHashMap<int, Entity> Prefabs = new NativeHashMap<int, Entity>(100, Allocator.Persistent);
        public NativeHashMap<int, EntityArchetype> PrefabEntityArchetypes = new NativeHashMap<int, EntityArchetype>(100, Allocator.Persistent);
        public Dictionary<GameObject, int> GameObjectIndex = new Dictionary<GameObject, int>();
        
        private readonly NetworkedPrefabs _networkedPrefabs;
        private readonly bool _isServer;

        public NetworkedPrefabSystem(NetworkedPrefabs networkedPrefabs, bool isServer)
        {
            _networkedPrefabs = networkedPrefabs;
            _isServer = isServer;
        }

        protected override void OnCreate()
        {
            var blobAssetStore = new BlobAssetStore();
            
            for (int i = 0; i < _networkedPrefabs.Prefabs.Count; i++)
            {
                GameObject prefab;
                
                if (_isServer)
                {
                    prefab = _networkedPrefabs.Prefabs[i].Server;
                }
                else
                {
                    prefab = _networkedPrefabs.Prefabs[i].Client;
                }

                if (prefab == null)
                    continue;
                
                var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, GameObjectConversionSettings.FromWorld(World, blobAssetStore));
                Prefabs[i] = entity;

                GameObjectIndex[prefab] = i;
                EntityManager.AddComponent<NetworkedPrefab>(entity);
                EntityManager.SetComponentData(entity, new NetworkedPrefab()
                {
                    Index = i
                });
                
                var types = EntityManager.GetComponentTypes(entity);
                PrefabEntityArchetypes[i] = EntityManager.CreateArchetype(types.ToArray());
                types.Dispose();
            }
            
            blobAssetStore.Dispose();
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            Prefabs.Dispose();
            PrefabEntityArchetypes.Dispose();
            base.OnDestroy();
        }

        public Entity GetEntityFromPrefab(GameObject prefab)
        {
            if (!GameObjectIndex.ContainsKey(prefab))
            {
                throw new KeyNotFoundException($"No networked prefab found for {prefab.name}");
            }

            return Prefabs[GameObjectIndex[prefab]];
        }

        protected override void OnUpdate()
        {
            
        }
    }
}
