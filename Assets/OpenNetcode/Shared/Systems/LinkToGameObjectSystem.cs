using System.Collections.Generic;
using OpenNetcode.Shared.Authoring;
using OpenNetcode.Shared.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace OpenNetcode.Shared.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPostSimulationSystemGroup))]
    public partial class LinkToGameObjectSystem : SystemBase
    {
        private int _spawnedIndex;
        Dictionary<int, GameObject> _spawned = new Dictionary<int, GameObject>();
        Dictionary<int, List<GameObject>> _pools = new Dictionary<int, List<GameObject>>();
        
        public struct LinkedToGameObjectTag : IComponentData
        {
            
        }

        public struct EntityLifetimeTracker : ISystemStateComponentData
        {
            public int Spawned;
            public int Type;
        }
        
        protected override void OnUpdate()
        {
            NativeHashMap<Entity, int> _prefabsToSpawn = new NativeHashMap<Entity, int>(1000, Allocator.Temp);
            var ecb = new EntityCommandBuffer(Allocator.Temp, PlaybackPolicy.SinglePlayback);
            
            Entities.WithNone<LinkedToGameObjectTag>().ForEach((ref Entity entity, in LinkToGameObject link) =>
            {
                ecb.AddComponent<EntityLifetimeTracker>(entity);
                ecb.AddComponent<LinkedToGameObjectTag>(entity);
                _prefabsToSpawn[entity] = link.Type;
            }).Run();

            Entities.WithNone<LinkedToGameObjectTag>().ForEach((ref Entity entity, in EntityLifetimeTracker link) =>
            {
                if (!_spawned.ContainsKey(link.Spawned))
                {
                    ecb.RemoveComponent<EntityLifetimeTracker>(entity);
                    return;
                    //Debug.LogWarning("Doesn't exits " + link.Spawned);
                }
                
                var go = _spawned[link.Spawned];
                go.SetActive(false);
                _pools[link.Type].Add(go);
                _spawned.Remove(link.Spawned);
                ecb.RemoveComponent<EntityLifetimeTracker>(entity);
            }).WithoutBurst().Run();
            
            ecb.Playback(EntityManager);

            foreach (var prefabToSpawn in _prefabsToSpawn)
            {
                int type = prefabToSpawn.Value;

                if (LinkToGameObjectAuthoring.PrefabsFromIndex.TryGetValue(type, out GameObject prefab))
                {
                    GameObject spawned = null;
                    LinkedGameObject linkedGameObject = null;

                    bool spawn = false;
                    
                    if (!_pools.ContainsKey(type))
                    {
                        var pool = new List<GameObject>();
                        _pools[type] = pool;
                        spawn = true;
                    }
                    else
                    {
                        var pool = _pools[type];
                        
                        if (pool.Count > 0)
                        {
                            spawned = pool[pool.Count - 1];
                            linkedGameObject = spawned.GetComponent<LinkedGameObject>();
                            spawned.SetActive(true);
                            _spawned[linkedGameObject.SpawnedIndex] = spawned;
                            //Debug.Log("Reactivated " + linkedGameObject.SpawnedIndex);
                            pool.RemoveAt(pool.Count - 1);
                        }
                        else
                        {
                            spawn = true;
                        }
                    }

                    if (spawn)
                    {
                        spawned = GameObject.Instantiate(prefab);
                        linkedGameObject = spawned.AddComponent<LinkedGameObject>();
                        linkedGameObject.SpawnedIndex = _spawnedIndex;
                        linkedGameObject.EntityManager = EntityManager;
                        linkedGameObject.World = World;
                        _spawned[_spawnedIndex] = spawned;
                        //Debug.Log("Spawned " + _spawnedIndex);
                        _spawnedIndex++;
                    }
                    
                    linkedGameObject.Entity = prefabToSpawn.Key;
                    
                    EntityManager.SetComponentData(prefabToSpawn.Key, new EntityLifetimeTracker()
                    {
                        Spawned = linkedGameObject.SpawnedIndex,
                        Type = prefabToSpawn.Value
                    });
                }
            }
        }
    }
}
