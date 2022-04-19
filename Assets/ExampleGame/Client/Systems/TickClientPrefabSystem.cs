using System.Collections.Generic;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace ExampleGame.Client.Systems
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickClientReceiveSystem))]
    [DisableAutoCreation]
    public partial class TickClientPrefabSystem : SystemBase
    {
        private readonly NetworkedPrefabs _networkedPrefabs;
        private readonly IClientNetworkSystem _client;
        private NetworkCompressionModel _compressionModel;
        private BlobAssetStore _blobAssetStore;
        private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        public NativeHashMap<int, Entity> PrefabEntities;
        public NativeHashMap<int, EntityArchetype> PrefabEntityArchetypes;
        

        public TickClientPrefabSystem(NetworkedPrefabs networkedPrefabs, IClientNetworkSystem client)
        {
            _networkedPrefabs = networkedPrefabs;
            _client = client;
        }

        protected override void OnCreate()
        {
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            PrefabEntities = new NativeHashMap<int, Entity>(100, Allocator.Persistent);
            PrefabEntityArchetypes = new NativeHashMap<int, EntityArchetype>(100, Allocator.Persistent);
            _blobAssetStore = new BlobAssetStore();

            foreach (var prefab in _networkedPrefabs.Client)
            {
                _prefabs.Add(prefab.name, prefab);
            }
        }

        protected override void OnDestroy()
        {
            PrefabEntities.Dispose();
            PrefabEntityArchetypes.Dispose();
            
            _blobAssetStore.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            foreach (var packet in _client.ReceivedPackets)
            {
                switch ((PacketType) packet.Key)
                {
                    case PacketType.NetworkedPrefabs:
                    {
                        PrefabEntities.Clear();
                        PrefabEntityArchetypes.Clear();
                        
                        var reader = packet.Value.Reader;

                        Packets.ReadPacketType(ref reader);
                        int count = (int) reader.ReadPackedUInt(_compressionModel);

                        for (int i = 0; i < count; i++)
                        {
                            int index = (int) reader.ReadPackedUInt(_compressionModel);
                            string prefabName = reader.ReadFixedString64().ToString();

                            if (!_prefabs.TryGetValue(prefabName, out GameObject prefab))
                            {
                                Debug.LogWarning($"Could not load client prefab {prefabName}");
                                continue;
                            }

                            var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, GameObjectConversionSettings.FromWorld(World, _blobAssetStore));
                            PrefabEntities.Add(index, entity);
                            
                            var types = EntityManager.GetComponentTypes(entity);
                            PrefabEntityArchetypes[i] = EntityManager.CreateArchetype(types.ToArray());
                            types.Dispose();
                        }
                        
                        Debug.Log($"Received {count} prefabs");
                        
                        break;
                    }
                }
            }
        }
    }
}
