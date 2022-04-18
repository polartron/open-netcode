using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
//</generated>

namespace ExampleGame.Server.Generated
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    public partial class TickServerPrefabSystem : SystemBase
    {
        private NativeHashMap<FixedString64Bytes, int> _prefabIndex;
        private int _index;

        private NetworkedPrefabs _networkedPrefabs;

        public TickServerPrefabSystem(NetworkedPrefabs networkedPrefabs)
        {
            _networkedPrefabs = networkedPrefabs;

        }

        protected override void OnCreate()
        {
            _prefabIndex = new NativeHashMap<FixedString64Bytes, int>(100, Allocator.Persistent);
            
            var blobAssetStore = new BlobAssetStore();

            DataStreamWriter writer = new DataStreamWriter(1000, Allocator.Persistent);
            NetworkCompressionModel compressionModel = new NetworkCompressionModel(Allocator.Temp);
            
            foreach (var prefab in _networkedPrefabs.Prefabs)
            {
                string guid = prefab.GetComponent<NetworkedPrefabBehaviour>().Guid;
                uint index = (uint) _index;
                _prefabIndex[guid] = _index++;
                
                var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, GameObjectConversionSettings.FromWorld(World, blobAssetStore));

                writer.WritePackedUInt(index, compressionModel);

                //<template:publicsnapshot>
                //if (EntityManager.HasComponent<##TYPE##>(entity))
                //{
                //    writer.WritePackedUInt(##INDEX##, compressionModel);
                //    var ##TYPELOWER## = EntityManager.GetComponentData<##TYPE##>(entity);
                //    ##TYPELOWER##.WriteSnapshot(ref writer, compressionModel, default);
                //}
                //</template>
                
                //<template:privatesnapshot>
                //if (EntityManager.HasComponent<##TYPE##>(entity))
                //{
                //    writer.WritePackedUInt(##INDEX##, compressionModel);
                //    var ##TYPELOWER## = EntityManager.GetComponentData<##TYPE##>(entity);
                //    ##TYPELOWER##.WriteSnapshot(ref writer, compressionModel, default);
                //}
                //</template>
            }
            
            blobAssetStore.Dispose();
            
            
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            _prefabIndex.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            Entities.WithNone<NetworkedPrefabIndex>().ForEach((ref Entity entity, in NetworkedPrefabGuid guid) =>
            {
                int index = 0;
                
                if (!_prefabIndex.TryGetValue(guid.Value, out index))
                {
                    index = _index;
                    _prefabIndex.Add(guid.Value, _index++);
                    Debug.Log("Test");
                    Debug.Log($"Added new prefab for GUID:{guid.Value} INDEX:{index}");
                }

                ecb.AddComponent<NetworkedPrefabIndex>(entity);
                ecb.SetComponent(entity, new NetworkedPrefabIndex()
                {
                    Value = index
                });
                
            }).WithoutBurst().Run();
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
