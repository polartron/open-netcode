using OpenNetcode.Server.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace ExampleGame.Server.Systems
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickServerReceiveSystem))]
    public partial class TickServerPrefabSystem : SystemBase
    {
        private NativeHashMap<FixedString64Bytes, int> _prefabIndex;
        private int _index;
        private readonly NetworkedPrefabs _networkedPrefabs;
        private readonly IServerNetworkSystem _server;
        private BlobAssetStore _blobAssetStore;
        private NativeArray<byte> _networkedPrefabData;
        private NativeHashMap<FixedString64Bytes, Entity> _prefabs;
        
        public TickServerPrefabSystem(NetworkedPrefabs networkedPrefabs, IServerNetworkSystem server)
        {
            _networkedPrefabs = networkedPrefabs;
            _server = server;
        }

        protected override void OnCreate()
        {
            _prefabIndex = new NativeHashMap<FixedString64Bytes, int>(100, Allocator.Persistent);
            _prefabs = new NativeHashMap<FixedString64Bytes, Entity>(100, Allocator.Persistent);
            _blobAssetStore = new BlobAssetStore();
            
            var blobAssetStore = new BlobAssetStore();

            DataStreamWriter writer = new DataStreamWriter(1000, Allocator.Temp);
            NetworkCompressionModel compressionModel = new NetworkCompressionModel(Allocator.Temp);
            Packets.WritePacketType(PacketType.NetworkedPrefabs, ref writer);

            var prefabs = _networkedPrefabs.Server;

            writer.WritePackedUInt((uint) prefabs.Count, compressionModel);
            
            foreach (var prefab in prefabs)
            {
                var serverPrefabAuthoring = prefab.GetComponent<ServerPrefabAuthoring>();

                string guid = serverPrefabAuthoring.Guid;
                uint index = (uint) _index;
                _prefabIndex[guid] = _index++;
                
                writer.WritePackedUInt(index, compressionModel);

                string clientPrefab = serverPrefabAuthoring.ClientPrefab;
                writer.WriteFixedString64(clientPrefab);
                
                var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, GameObjectConversionSettings.FromWorld(World, _blobAssetStore));
                _prefabs.Add(new FixedString64Bytes(serverPrefabAuthoring.name), entity);
            }
            
            _networkedPrefabData = new NativeArray<byte>(writer.AsNativeArray(), Allocator.Persistent);
            blobAssetStore.Dispose();
            base.OnCreate();
        }

        public void SendNetworkedPrefabs(int networkId)
        {
            _server.Send(networkId, Packets.WrapPacket(_networkedPrefabData, _networkedPrefabData.Length));
        }

        public Entity SpawnPrefab(FixedString64Bytes name)
        {
            if (!_prefabs.TryGetValue(new FixedString64Bytes(name), out Entity prefab))
            {
                Debug.LogWarning($"Could not find prefab with name {name}");
                return Entity.Null;
            }
            
            Entity entity = EntityManager.Instantiate(prefab);
            
#if UNITY_EDITOR
            EntityManager.SetName(entity, EntityManager.GetName(prefab));
#endif
            return entity;
        }

        protected override void OnDestroy()
        {
            _blobAssetStore.Dispose();
            _prefabIndex.Dispose();
            _networkedPrefabData.Dispose();
            _prefabs.Dispose();
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
