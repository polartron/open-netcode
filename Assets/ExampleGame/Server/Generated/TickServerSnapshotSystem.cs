using ExampleGame.Server.Generated;
using ExampleGame.Shared.Components;
using OpenNetcode.Server.Components;
using OpenNetcode.Server.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using OpenNetcode.Shared.Utils;
using Shared.Utils;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Transforms;
using Shared.Generated;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    internal struct PlayerSnapshot
    {
        public int PlayerId;
        public int SnapshotIndex;
    }

    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPostSimulationSystemGroup))]
    [UpdateBefore(typeof(TickServerSendSystem))]
    public unsafe class TickServerSnapshotSystem : SystemBase
    {
        private IServerNetworkSystem _server;
        private NativeHashMap<int, Area> _areas;
        private NetworkCompressionModel _compressionModel;
        private EntityQuery _entitiesQuery;
        private EntityQuery _playersQuery;

        public TickServerSnapshotSystem(IServerNetworkSystem server)
        {
            _server = server;
        }

        protected override void OnCreate()
        {
            _areas = new NativeHashMap<int, Area>(1000, Allocator.Persistent);
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            _entitiesQuery = GetEntityQuery(
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<NetworkedPrefab>(),
                ComponentType.ReadOnly<ServerNetworkedEntity>(),
                ComponentType.ReadOnly<SpatialHash>());
            _playersQuery = GetEntityQuery(
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<ServerNetworkedEntity>(),
                ComponentType.ReadOnly<PrivateSnapshotObserver>(),
                ComponentType.ReadOnly<SpatialHash>(),
                ComponentType.ReadWrite<NetworkedPrefab>(),
                ComponentType.ReadWrite<PlayerBaseLine>());

            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            foreach (var area in _areas)
            {
                area.Value.Dispose();
            }

            _areas.Dispose();
            _compressionModel.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            int tick = GetSingleton<TickData>().Value;

            if (tick % (TimeConfig.TicksPerSecond / TimeConfig.SnapshotsPerSecond) != 0)
            {
                return;
            }

            int snapshotIndex = tick / (TimeConfig.TicksPerSecond / TimeConfig.SnapshotsPerSecond);
            int entitiesCount = _entitiesQuery.CalculateEntityCount();
            int playersCount = _playersQuery.CalculateEntityCount();

            var activeAreasList = new NativeHashSet<int>(playersCount, Allocator.TempJob);
            var entitiesInAreas = new NativeMultiHashMap<int, ServerEntitySnapshot>(entitiesCount, Allocator.TempJob);
            var playersInAreas = new NativeMultiHashMap<int, PlayerSnapshot>(playersCount, Allocator.TempJob);
            var currentPlayerSnapshots = new NativeMultiHashMap<int, PlayerSnapshot>(playersCount, Allocator.Temp);

            var spatialHashHandle = GetComponentTypeHandle<SpatialHash>(true);

            {
                var privateSnapshotBuffers = new NativeArray<PacketArrayWrapper>(playersCount, Allocator.TempJob);
                var privateSnapshotResults = new NativeHashMap<int, PacketArrayWrapper>(playersCount, Allocator.TempJob);
                var bufferOfArraysPrivate = new NativeArray<byte>(SnapshotSettings.PrivateSnapshotSize * playersCount, Allocator.TempJob);
                
                for (int i = 0; i < privateSnapshotBuffers.Length; i++)
                {
                    privateSnapshotBuffers[i] = new PacketArrayWrapper()
                    {
                        Pointer = bufferOfArraysPrivate.GetSubArray(i * SnapshotSettings.PrivateSnapshotSize, SnapshotSettings.PrivateSnapshotSize).GetUnsafePtr(),
                        Allocator = Allocator.TempJob,
                        Length = SnapshotSettings.PrivateSnapshotSize
                    };
                }

                PrivateSnapshotJob privateSnapshotJob = new PrivateSnapshotJob()
                {
                    PacketArray = privateSnapshotBuffers.GetUnsafePtr(),
                    PacketsLength = privateSnapshotBuffers.Length,
                    Results = privateSnapshotResults.AsParallelWriter(),
                    Tick = tick,
                    ActiveAreasList = activeAreasList.AsParallelWriter(),
                    CompressionModel = _compressionModel,
                    PlayersInArea = playersInAreas.AsParallelWriter(),
                    ServerNetworkedEntityHandle = GetComponentTypeHandle<ServerNetworkedEntity>(true),
                    PrivateSnapshotObserverHandle = GetBufferTypeHandle<PrivateSnapshotObserver>(true),
                    NetworkedPrefabFromEntityHandle = GetComponentDataFromEntity<NetworkedPrefab>(true),
                    PlayerBaseLineHandle = GetComponentTypeHandle<PlayerBaseLine>(),
                    SpatialHashHandle = spatialHashHandle,
                    //<template>
                    //##TYPE##Components = GetComponentDataFromEntity<##TYPE##>(true),
                    //</template>
//<generated>
                    EntityPositionComponents = GetComponentDataFromEntity<EntityPosition>(true),
                    EntityVelocityComponents = GetComponentDataFromEntity<EntityVelocity>(true),
                    PathComponentComponents = GetComponentDataFromEntity<PathComponent>(true),
//</generated>
                    //<privatetemplate>
                    //##TYPE##Components = GetComponentDataFromEntity<##TYPE##>(true),
                    //</privatetemplate>
//<generated>
                    EntityHealthComponents = GetComponentDataFromEntity<EntityHealth>(true),
//</generated>
                };

                Dependency = privateSnapshotJob.ScheduleParallel(_playersQuery, Dependency);
                Dependency.Complete();

                foreach (var result in privateSnapshotResults)
                {
                    _server.Send(result.Value.InternalId, Packets.WrapPacket(result.Value.GetArray<byte>(), result.Value.Length));
                }
                
                bufferOfArraysPrivate.Dispose();
                privateSnapshotBuffers.Dispose();
                privateSnapshotResults.Dispose();

                foreach (var area in activeAreasList)
                {
                    if (!_areas.ContainsKey(area))
                    {
                        _areas.Add(area, new Area(SnapshotSettings.MaxEntititesInArea));
                    }
                }
            }

            AddEntitiesToAreas addEntitiesToAreas = new AddEntitiesToAreas()
            {
                ActiveAreas = activeAreasList,
                EntitiesInAreas = entitiesInAreas.AsParallelWriter(),
                NetworkedPrefabHandle = GetComponentTypeHandle<NetworkedPrefab>(true),
                EntityTypeHandle = GetEntityTypeHandle(),
                SpatialHashHandle = spatialHashHandle,
                //<template>
                //##TYPE##Components = GetComponentDataFromEntity<##TYPE##>(true),
                //</template>
//<generated>
                EntityPositionComponents = GetComponentDataFromEntity<EntityPosition>(true),
                EntityVelocityComponents = GetComponentDataFromEntity<EntityVelocity>(true),
                PathComponentComponents = GetComponentDataFromEntity<PathComponent>(true),
//</generated>
                //<events>
                //##TYPE##BufferFromEntity = GetBufferFromEntity<BumpEvent>(true),
                //</events>
//<generated>
                BumpEventBufferFromEntity = GetBufferFromEntity<BumpEvent>(true),
//</generated>
            };

            Dependency = addEntitiesToAreas.ScheduleParallel(_entitiesQuery, 4, Dependency);
            Dependency.Complete();

            var activeAreas = activeAreasList.ToNativeArray(Allocator.Temp);
            int jobIndex = 0;

            int playersInAreasCount = playersInAreas.Count();
            
            NativeHashMap<int, PacketArrayWrapper> publicSnapshotResults = new NativeHashMap<int, PacketArrayWrapper>(playersInAreasCount, Allocator.TempJob);
            NativeList<JobHandle> jobs = new NativeList<JobHandle>(playersInAreasCount, Allocator.Temp);
            NativeArray<PacketArrayWrapper> buffers = new NativeArray<PacketArrayWrapper>(playersInAreasCount, Allocator.TempJob);
            NativeArray<byte> bufferOfArraysPublic = new NativeArray<byte>(playersInAreasCount * SnapshotSettings.PublicSnapshotSize, Allocator.TempJob);
            
            for (int i = 0; i < activeAreas.Length; i++)
            {
                int hash = activeAreas[i];

                NativeHashMap<int, int> snapshotJobs = new NativeHashMap<int, int>(playersInAreasCount, Allocator.Temp);
                NativeHashMap<int, int> jobsForPlayers = new NativeHashMap<int, int>(playersInAreasCount, Allocator.Temp);

                foreach (var player in playersInAreas.GetValuesForKey(hash))
                {
                    int offset = snapshotIndex - player.SnapshotIndex;

                    if (snapshotJobs.ContainsKey(offset))
                    {
                        //Use already sent snapshot
                        jobsForPlayers.Add(player.PlayerId, snapshotJobs[offset]);
                    }
                    else
                    {
                        buffers[jobIndex] = new PacketArrayWrapper()
                        {
                            Allocator = Allocator.TempJob,
                            Pointer = bufferOfArraysPublic.GetSubArray(SnapshotSettings.PublicSnapshotSize * jobIndex, SnapshotSettings.PublicSnapshotSize).GetUnsafePtr(),
                            Length = SnapshotSettings.PublicSnapshotSize
                        };
                        
                        MakeSnapshotJob snapshotJob = new MakeSnapshotJob()
                        {
                            Buffers = buffers.GetUnsafePtr(),
                            BuffersLength = buffers.Length,
                            Area = _areas[hash],
                            Hash = hash,
                            SnapshotIndex = snapshotIndex,
                            PlayerSnapshotIndex = player.SnapshotIndex,
                            PlayerSnapshots = entitiesInAreas,
                            //<template>
                            //##TYPE##FromEntity = GetComponentDataFromEntity<##TYPE##>(true),
                            //</template>
//<generated>
                            EntityPositionFromEntity = GetComponentDataFromEntity<EntityPosition>(true),
                            EntityVelocityFromEntity = GetComponentDataFromEntity<EntityVelocity>(true),
                            PathComponentFromEntity = GetComponentDataFromEntity<PathComponent>(true),
//</generated>
                            //<events>
                            //##TYPE##BufferFromEntity = GetBufferFromEntity<##TYPE##>(true),
                            //</events>
//<generated>
                            BumpEventBufferFromEntity = GetBufferFromEntity<BumpEvent>(true),
//</generated>
                            CompressionModel = _compressionModel,
                            JobIndex = jobIndex++,
                            Results = publicSnapshotResults.AsParallelWriter()
                        };

                        jobs.Add(snapshotJob.Schedule());
                        jobsForPlayers.Add(player.PlayerId, snapshotJob.JobIndex);
                        snapshotJobs.Add(offset, snapshotJob.JobIndex);
                    }
                }

                jobsForPlayers.Dispose();
                snapshotJobs.Dispose();
            }

            for (int i = 0; i < jobs.Length; i++)
            {
                jobs[i].Complete();
            }

            foreach (var result in publicSnapshotResults)
            {
                _server.Send(result.Key, Packets.WrapPacket(result.Value.GetArray<byte>(), result.Value.Length));
            }

            ClearEventsJob clearEventsJob = new ClearEventsJob()
            {
                EntityTypeHandle = GetEntityTypeHandle(),
                //<events>
                //##TYPE##BufferFromEntity = GetBufferFromEntity<##TYPE##>()
                //</events>
//<generated>
                BumpEventBufferFromEntity = GetBufferFromEntity<BumpEvent>()
//</generated>
            };
            
            Dependency = clearEventsJob.Schedule(_entitiesQuery);
            Dependency.Complete();

            bufferOfArraysPublic.Dispose();
            buffers.Dispose();
            activeAreas.Dispose();
            activeAreasList.Dispose();
            entitiesInAreas.Dispose();
            playersInAreas.Dispose();
            currentPlayerSnapshots.Dispose();
            publicSnapshotResults.Dispose();
        }
    }
}