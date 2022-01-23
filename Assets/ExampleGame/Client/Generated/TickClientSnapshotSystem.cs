using System;
using ExampleGame.Shared.Components;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using OpenNetcode.Client.Components;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
using ExampleGame.Shared.Components;
//</generated>

//<template>
//[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<##TYPE##>))]
//</template>
//<generated>
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<EntityPosition>))]
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<EntityVelocity>))]
//</generated>
//<privatetemplate>
//[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<##TYPE##>))]
//</privatetemplate>
//<generated>
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<EntityHealth>))]
//</generated>
//<events>
//[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<##TYPE##>))]
//</events>
//<generated>
[assembly: RegisterGenericComponentType(typeof(SnapshotBufferElement<BumpEvent>))]
//</generated>
namespace Client.Generated
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderFirst = true)]
    [UpdateAfter(typeof(TickClientReceiveSystem))]
    public class TickClientSnapshotSystem<TPrediction, TInput> : SystemBase
        where TPrediction : unmanaged, INetworkedComponent
        where TInput : unmanaged, INetworkedComponent
    {
        private IClientNetworkSystem _client;
        private NativeHashMap<int, ClientEntitySnapshot> _snapshotEntities;
        private NativeHashMap<int, ClientEntitySnapshot> _observedEntities;
        private NativeHashMap<int, ClientArea> _areas;
        private NetworkedPrefabSystem _networkedPrefabSystem;
        private NetworkCompressionModel _compressionModel;
        private EntityQuery _linkEntitiesQuery;

        public TickClientSnapshotSystem(IClientNetworkSystem client)
        {
            _client = client;
        }

        protected override void OnCreate()
        {
            World.GetExistingSystem<TickPredictionSystem<TPrediction, TInput>>().OnRollback += Rollback;
            _networkedPrefabSystem = World.GetExistingSystem<NetworkedPrefabSystem>();
            _areas = new NativeHashMap<int, ClientArea>(1000, Allocator.Persistent);
            _snapshotEntities = new NativeHashMap<int, ClientEntitySnapshot>(10000, Allocator.Persistent);
            _observedEntities = new NativeHashMap<int, ClientEntitySnapshot>(10000, Allocator.Persistent);
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            _linkEntitiesQuery = GetEntityQuery(ComponentType.ReadOnly<ServerEntity>());

            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            foreach (var area in _areas)
            {
                area.Value.Dispose();
            }

            _snapshotEntities.Dispose();
            _observedEntities.Dispose();
            _areas.Dispose();

            base.OnDestroy();
        }

        public void Rollback(Entity entity, int tick)
        {
            //<template>
            //DoRollback<##TYPE##>(entity, tick);
            //</template>
//<generated>
            DoRollback<EntityPosition>(entity, tick);
            DoRollback<EntityVelocity>(entity, tick);
//</generated>
        }

        private void DoRollback<T>(in Entity entity, int tick) where T : unmanaged, INetworkedComponent
        {
            if (!EntityManager.HasComponent<SnapshotBufferElement<T>>(entity))
                return;

            var buffer = EntityManager.GetBuffer<SnapshotBufferElement<T>>(entity);
            var component = buffer[tick % buffer.Length];

            if (component.Tick == tick)
            {
                EntityManager.SetComponentData(entity, component.Value);
            }
        }

        protected override void OnUpdate()
        {
            foreach (var packet in _client.ReceivedPackets)
            {
                switch ((PacketType) packet.Key)
                {
                    case PacketType.PublicSnapshot:
                    {
                        var data = packet.Value.GetArray<byte>();

                        bool success = ReadPublicSnapshotJob(data, out int latestSnapshotIndex, out int latestSnapshotTick);

                        var clientData = GetSingleton<ClientData>();

                        if (success)
                        {
                            if (latestSnapshotIndex > clientData.LastReceivedSnapshotIndex)
                            {
                                clientData.LastReceivedSnapshotIndex = latestSnapshotIndex;
                                clientData.LastReceivedSnapshotTick = latestSnapshotTick;
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Failed to parse public snapshot. Resetting.");

                            //Reset back to zero for now
                            //TODO: Just clear the entities that are not a part of the current baseline and continue

                            foreach (var area in _areas)
                            {
                                area.Value.Dispose();
                            }

                            foreach (var snapshot in _snapshotEntities)
                            {
                                if (!_observedEntities.ContainsKey(snapshot.Key) &&
                                    snapshot.Key != clientData.LocalPlayerServerEntityId)
                                {
                                    EntityManager.DestroyEntity(snapshot.Value.Entity);
                                }
                            }

                            _snapshotEntities.Clear();
                            _areas.Clear();
                            clientData.Resetting = true;
                            clientData.LastReceivedSnapshotIndex = 0;
                            clientData.LastReceivedSnapshotTick = 0;
                        }

                        SetSingleton(clientData);

                        break;
                    }

                    case PacketType.PrivateSnapshot:
                    {
                        var data = packet.Value.GetArray<byte>();
                        ReadPrivateSnapshot(data);
                        break;
                    }
                }
            }
        }

        private void ReadBufferElement<T>(ref DataStreamReader reader, in Entity entity, int tick) where T : unmanaged, ISnapshotComponent<T>, IComponentData
        {
            T component = new T();
            component.ReadSnapshot(ref reader, _compressionModel, default);

            var buffer = EntityManager.GetBuffer<SnapshotBufferElement<T>>(entity);
            buffer[tick % buffer.Length] = new SnapshotBufferElement<T>()
            {
                Value = component,
                Tick = tick
            };
        }

        private void ReadPrivateSnapshot(in NativeArray<byte> data)
        {
            var clientData = GetSingleton<ClientData>();

            var reader = new DataStreamReader(data);
            Packets.ReadPacketType(ref reader);
            int snapshotTick = (int) reader.ReadPackedUInt(_compressionModel);
            int version = (int) reader.ReadPackedUInt(_compressionModel);
            int observedEntities = (int) reader.ReadPackedUInt(_compressionModel);

            clientData.Version = version;
            SetSingleton(clientData);

            var prefabs = _networkedPrefabSystem.Prefabs;

            for (int i = 0; i < observedEntities; i++)
            {
                int index = (int) reader.ReadPackedUInt(_compressionModel);
                int type = reader.ReadPackedInt(_compressionModel);
                int updateMask = reader.ReadPackedInt(_compressionModel);

                if (!_observedEntities.ContainsKey(index))
                {
                    Entity entity;
                    
                    if (index == clientData.LocalPlayerServerEntityId)
                    {
                        entity = clientData.LocalPlayer;
                    }
                    else if (_snapshotEntities.ContainsKey(index))
                    {
                        entity = _snapshotEntities[index].Entity;
                    }
                    else
                    {
                        entity = EntityManager.Instantiate(prefabs[type]);
                        EntityManager.RemoveComponent<Prefab>(entity);
                    }
                    
                    int componentMask = 0;

                    //<template>
                    //if (EntityManager.HasComponent(entity, typeof(##TYPE##)))
                    //{
                    //    componentMask = componentMask | (1 << ##INDEX##);
                    //}
                    //</template>
//<generated>
                    if (EntityManager.HasComponent(entity, typeof(EntityPosition)))
                    {
                        componentMask = componentMask | (1 << 0);
                    }
                    if (EntityManager.HasComponent(entity, typeof(EntityVelocity)))
                    {
                        componentMask = componentMask | (1 << 1);
                    }
//</generated>
                    //<privatetemplate>
                    //if (EntityManager.HasComponent(entity, typeof(##TYPE##)))
                    //{
                    //    componentMask = componentMask | (1 << ##INDEX##);
                    //}
                    //</privatetemplate>
//<generated>
                    if (EntityManager.HasComponent(entity, typeof(EntityHealth)))
                    {
                        componentMask = componentMask | (1 << 2);
                    }
//</generated>

                    //<template>
                    //if (!EntityManager.HasComponent<SnapshotBufferElement<##TYPE##>>(entity))
                    //{
                    //    var buffer = EntityManager.AddBuffer<SnapshotBufferElement<##TYPE##>>(entity);
                    //    for (int b = 0; b < TimeConfig.SnapshotsPerSecond; b++)
                    //        buffer.Add(default);
                    //}
                    //</template>
//<generated>
                    if (!EntityManager.HasComponent<SnapshotBufferElement<EntityPosition>>(entity))
                    {
                        var buffer = EntityManager.AddBuffer<SnapshotBufferElement<EntityPosition>>(entity);
                        for (int b = 0; b < TimeConfig.SnapshotsPerSecond; b++)
                            buffer.Add(default);
                    }
                    if (!EntityManager.HasComponent<SnapshotBufferElement<EntityVelocity>>(entity))
                    {
                        var buffer = EntityManager.AddBuffer<SnapshotBufferElement<EntityVelocity>>(entity);
                        for (int b = 0; b < TimeConfig.SnapshotsPerSecond; b++)
                            buffer.Add(default);
                    }
//</generated>
                    //<privatetemplate>
                    //if (!EntityManager.HasComponent<SnapshotBufferElement<##TYPE##>>(entity))
                    //{
                    //    var buffer = EntityManager.AddBuffer<SnapshotBufferElement<##TYPE##>>(entity);
                    //    for (int b = 0; b < TimeConfig.SnapshotsPerSecond; b++)
                    //        buffer.Add(default);
                    //}
                    //</privatetemplate>
//<generated>
                    if (!EntityManager.HasComponent<SnapshotBufferElement<EntityHealth>>(entity))
                    {
                        var buffer = EntityManager.AddBuffer<SnapshotBufferElement<EntityHealth>>(entity);
                        for (int b = 0; b < TimeConfig.SnapshotsPerSecond; b++)
                            buffer.Add(default);
                    }
//</generated>
                    var clientEntitySnapshot = new ClientEntitySnapshot()
                    {
                        Entity = entity,
                        ServerId = index,
                        ComponentMask = componentMask
                    };
                    
                    _observedEntities.Add(index, clientEntitySnapshot);
                }

                ClientEntitySnapshot observedSnapshot = _observedEntities[index];

                //<template>
                //if ((updateMask & (1 << ##INDEX##)) != 0)
                //{
                //    ReadBufferElement<##TYPE##>(ref reader, observedSnapshot.Entity, snapshotTick);
                //}
                //</template>
//<generated>
                if ((updateMask & (1 << 0)) != 0)
                {
                    ReadBufferElement<EntityPosition>(ref reader, observedSnapshot.Entity, snapshotTick);
                }
                if ((updateMask & (1 << 1)) != 0)
                {
                    ReadBufferElement<EntityVelocity>(ref reader, observedSnapshot.Entity, snapshotTick);
                }
//</generated>
                //<privatetemplate>
                //if ((updateMask & (1 << ##INDEX##)) != 0)
                //{
                //    ReadBufferElement<##TYPE##>(ref reader, observedSnapshot.Entity, snapshotTick);
                //}
                //</privatetemplate>
//<generated>
                if ((updateMask & (1 << 2)) != 0)
                {
                    ReadBufferElement<EntityHealth>(ref reader, observedSnapshot.Entity, snapshotTick);
                }
//</generated>
            }
        }

        public bool ReadPublicSnapshotJob(in NativeArray<byte> data, out int latestSnapshotIndex,
            out int latestSnapshotTick)
        {
            var reader = new DataStreamReader(data);
            Packets.ReadPacketType(ref reader);
            int snapshotIndex = (int) reader.ReadPackedUInt(_compressionModel); // SnapshotIndex
            int hash = reader.ReadPackedInt(_compressionModel); // Hash
            int baseLine = (int) reader.ReadPackedUInt(_compressionModel); // BaseLine
            int tick = snapshotIndex * (TimeConfig.TicksPerSecond / TimeConfig.SnapshotsPerSecond);

            var clientData = GetSingleton<ClientData>();

            if (clientData.Resetting && baseLine > 0)
            {
                latestSnapshotIndex = 0;
                latestSnapshotTick = 0;
                SetSingleton(clientData);
                return true;
            }

            if (clientData.Resetting && baseLine == 0)
            {
                clientData.Resetting = false;
            }

            SetSingleton(clientData);

            if (!_areas.ContainsKey(hash))
            {
                _areas.Add(hash, new ClientArea(10000));
            }

            var ecb = new EntityCommandBuffer(Allocator.TempJob, PlaybackPolicy.SinglePlayback);

            NativeArray<bool> parseJobSuccessful = new NativeArray<bool>(1, Allocator.TempJob);
            
            var parseSnapshotJob = new ParsePublicSnapshotJob()
            {
                Success = parseJobSuccessful,
                Area = _areas[hash],
                Snapshot = data,
                ClientData = clientData,
                CompressionModel = _compressionModel,
                EntityArchetypes = _networkedPrefabSystem.PrefabEntityArchetypes,
                Prefabs = _networkedPrefabSystem.Prefabs,
                ObservedEntities = _observedEntities,
                SnapshotEntities = _snapshotEntities,
                EntityCommandBuffer = ecb,
                //<template>
                //##TYPE##Buffer = GetBufferFromEntity<SnapshotBufferElement<##TYPE##>>(),
                //</template>
//<generated>
                EntityPositionBuffer = GetBufferFromEntity<SnapshotBufferElement<EntityPosition>>(),
                EntityVelocityBuffer = GetBufferFromEntity<SnapshotBufferElement<EntityVelocity>>(),
//</generated>
                //<privatetemplate>
                //##TYPE##Buffer = GetBufferFromEntity<SnapshotBufferElement<##TYPE##>>(),
                //</privatetemplate>
//<generated>
                EntityHealthBuffer = GetBufferFromEntity<SnapshotBufferElement<EntityHealth>>(),
//</generated>
                //<events>
                //##TYPE##Buffer = GetBufferFromEntity<SnapshotBufferElement<##TYPE##>>(),
                //</events>
//<generated>
                BumpEventBuffer = GetBufferFromEntity<SnapshotBufferElement<BumpEvent>>(),
//</generated>
            };
            
            Dependency = parseSnapshotJob.Schedule(Dependency);
            Dependency.Complete();

            ecb.Playback(EntityManager);
            ecb.Dispose();

            Dependency.Complete();

            var entities = _linkEntitiesQuery.ToEntityArray(Allocator.TempJob);
            var linkJob = new LinkCreatedEntities()
            {
                ObservedEntities = _observedEntities,
                SnapshotEntities = _snapshotEntities,
                Entities = entities,
                ServerEntityComponents = GetComponentDataFromEntity<ServerEntity>(true)
            };

            Dependency = linkJob.Schedule(Dependency);
            Dependency.Complete();
            entities.Dispose();

            if (!parseJobSuccessful[0])
            {
                latestSnapshotIndex = clientData.LastReceivedSnapshotIndex;
                latestSnapshotTick = clientData.LastReceivedSnapshotTick;
                parseJobSuccessful.Dispose();

                return false;
            }
            
            parseJobSuccessful.Dispose();

            var makeBaseLineJob = new MakeBaseLineJob()
            {
                Area = _areas[hash],
                Entities = _snapshotEntities,
                SnapshotIndex = snapshotIndex,
                Tick = tick,
                NetworkedPrefabFromEntity = GetComponentDataFromEntity<NetworkedPrefab>(true),
                //<template>
                //##TYPE##Buffer = GetBufferFromEntity<SnapshotBufferElement<##TYPE##>>(true),
                //</template>
//<generated>
                EntityPositionBuffer = GetBufferFromEntity<SnapshotBufferElement<EntityPosition>>(true),
                EntityVelocityBuffer = GetBufferFromEntity<SnapshotBufferElement<EntityVelocity>>(true),
//</generated>
            };
            
            Dependency = makeBaseLineJob.Schedule(Dependency);
            Dependency.Complete();

            latestSnapshotIndex = snapshotIndex;
            latestSnapshotTick = tick;
            return true;
        }
    }
}