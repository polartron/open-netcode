using OpenNetcode.Server.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using OpenNetcode.Shared.Utils;
using OpenNetcode.Server.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickServerReceiveSystem))]
    public class TickInputBufferSystem : SystemBase
    {
        public struct InputBufferData
        {
            //<template:input>
            //public ##TYPE## ##TYPE##;
            //</template>
//<generated>
            public MovementInput MovementInput;
            public WeaponInput WeaponInput;
//</generated>
            public double ArrivedTime;
            public int Tick;
            public int Version;
            public int BaseLine;
        }

        private IServerNetworkSystem _server;
        private NativeHashMap<int, int> _connectionIndex;
        private NativeList<int> _freeIndexes;
        private NativeArray<InputBufferData> _inputs;
        private EntityQuery _playersQuery;
        private NetworkCompressionModel _compressionModel;

        public TickInputBufferSystem(IServerNetworkSystem server)
        {
            _server = server;
        }

        protected override void OnCreate()
        {
            int maxPlayers = 100;
            
            _connectionIndex = new NativeHashMap<int, int>(maxPlayers, Allocator.Persistent);
            _freeIndexes = new NativeList<int>(maxPlayers, Allocator.Persistent);
            for (int i = 0; i < _freeIndexes.Capacity; i++)
            {
                _freeIndexes.Add(i);
            }
            
            _inputs  = new NativeArray<InputBufferData>(TimeConfig.TicksPerSecond * maxPlayers, Allocator.Persistent);
            _playersQuery = GetEntityQuery(
                //<template:input>
                //ComponentType.ReadOnly<##TYPE##>(),
                //</template>
//<generated>
                ComponentType.ReadOnly<MovementInput>(),
                ComponentType.ReadOnly<WeaponInput>(),
//</generated>
                ComponentType.ReadOnly<ProcessedInput>(),
                ComponentType.ReadOnly<PlayerControlledTag>(),
                ComponentType.ReadOnly<ServerNetworkedEntity>()

            );

            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);

            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            _connectionIndex.Dispose();
            _freeIndexes.Dispose();
            _inputs.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            var packets = _server.ReceivePackets;
            
            if(packets.TryGetFirstValue((int) PacketType.Input, out PacketArrayWrapper wrapper, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    var array = wrapper.GetArray<byte>();
                    var reader = new DataStreamReader(array);
                    if (!ReadInput(ref reader, _compressionModel, wrapper.InternalId))
                    {
                        Debug.Log("Failed");
                    }

                } while (packets.TryGetNextValue(out wrapper, ref iterator));
            }

            NativeHashMap<int, int> connectionIndex = _connectionIndex;
            NativeArray<InputBufferData> inputs = _inputs;
            
            UpdatePlayerInputJob job = new UpdatePlayerInputJob()
            {
                inputs = inputs,
                connectionIndex = connectionIndex,
                tick = GetSingleton<TickData>().Value,
                NetworkEntityTypeHandle = GetComponentTypeHandle<ServerNetworkedEntity>(true),
                ProcessedInputTypeHandle = GetBufferTypeHandle<ProcessedInput>(),
                PlayerBaseLineTypeHandle = GetComponentTypeHandle<PlayerBaseLine>(),
                //<template:input>
                //##TYPE##TypeHandle = GetComponentTypeHandle<##TYPE##>(),
                //</template>
//<generated>
                MovementInputTypeHandle = GetComponentTypeHandle<MovementInput>(),
                WeaponInputTypeHandle = GetComponentTypeHandle<WeaponInput>(),
//</generated>
            };
            
            Dependency = job.ScheduleParallel(_playersQuery, 4, Dependency);
            Dependency.Complete();
        }

        public bool ReadInput(ref DataStreamReader reader, in NetworkCompressionModel compressionModel, int internalId)
        {
            Packets.ReadPacketType(ref reader);
            int count = (int) reader.ReadRawBits(3);
            int tick = (int) reader.ReadPackedUInt(compressionModel);
            int lastReceivedSnapshotTick = (int) reader.ReadPackedUInt(compressionModel);
            int version = (int) reader.ReadPackedUInt(compressionModel);

            if (!_connectionIndex.ContainsKey(internalId))
            {
                _connectionIndex[internalId] = _freeIndexes[0];
                _freeIndexes.RemoveAt(0);
            }

            int offset = _connectionIndex[internalId] * TimeConfig.TicksPerSecond;

            //<template:input>
            //##TYPE## last##TYPE## = new ##TYPE##();
            //</template>
//<generated>
            MovementInput lastMovementInput = new MovementInput();
            WeaponInput lastWeaponInput = new WeaponInput();
//</generated>
            
            for (int i = 0; i < count; i++)
            {
                //<template:input>
                //##TYPE## ##TYPELOWER## = new ##TYPE##();
                //##TYPELOWER##.ReadSnapshot(ref reader, _compressionModel, last##TYPE##);
                //last##TYPE## = ##TYPELOWER##;
                //</template>
//<generated>
                MovementInput movementInput = new MovementInput();
                movementInput.ReadSnapshot(ref reader, _compressionModel, lastMovementInput);
                lastMovementInput = movementInput;
                WeaponInput weaponInput = new WeaponInput();
                weaponInput.ReadSnapshot(ref reader, _compressionModel, lastWeaponInput);
                lastWeaponInput = weaponInput;
//</generated>
                
                int index = (tick - i + TimeConfig.TicksPerSecond) % TimeConfig.TicksPerSecond;

                if (_inputs[offset + index].Tick != tick - i)
                {
                    var inputBufferData = _inputs[offset + index];
                    //<template:input>
                    //inputBufferData.##TYPE## = last##TYPE##;
                    //</template>
//<generated>
                    inputBufferData.MovementInput = lastMovementInput;
                    inputBufferData.WeaponInput = lastWeaponInput;
//</generated>
                    inputBufferData.ArrivedTime = Time.ElapsedTime;
                    inputBufferData.Tick = tick - i;
                    inputBufferData.Version = version;
                    inputBufferData.BaseLine = lastReceivedSnapshotTick;
                    _inputs[offset + index] = inputBufferData;
                }
            }

            return !reader.HasFailedReads;
        }

        [BurstCompile]
        public struct UpdatePlayerInputJob : IJobEntityBatch
        {
            [ReadOnly] public NativeArray<InputBufferData> inputs;
            [ReadOnly] public NativeHashMap<int, int> connectionIndex;
            [ReadOnly] public int tick;
            
            //<template:input>
            //public ComponentTypeHandle<##TYPE##> ##TYPE##TypeHandle;
            //</template>
//<generated>
            public ComponentTypeHandle<MovementInput> MovementInputTypeHandle;
            public ComponentTypeHandle<WeaponInput> WeaponInputTypeHandle;
//</generated>
            public ComponentTypeHandle<PlayerBaseLine> PlayerBaseLineTypeHandle;
            public BufferTypeHandle<ProcessedInput> ProcessedInputTypeHandle;
            [ReadOnly] public ComponentTypeHandle<ServerNetworkedEntity> NetworkEntityTypeHandle;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                //<template:input>
                //var chunk##TYPE## = batchInChunk.GetNativeArray(##TYPE##TypeHandle);
                //</template>
//<generated>
                var chunkMovementInput = batchInChunk.GetNativeArray(MovementInputTypeHandle);
                var chunkWeaponInput = batchInChunk.GetNativeArray(WeaponInputTypeHandle);
//</generated>
                var chunkPlayerBaseLines = batchInChunk.GetNativeArray(PlayerBaseLineTypeHandle);
                var chunkProcessedInput = batchInChunk.GetBufferAccessor(ProcessedInputTypeHandle);
                var chunkNetworkEntities = batchInChunk.GetNativeArray(NetworkEntityTypeHandle);

                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var networkEntity = chunkNetworkEntities[i];
                    var processedInputs = chunkProcessedInput[i];
                    
                    if (!connectionIndex.ContainsKey(networkEntity.OwnerNetworkId))
                        return;

                    int offset = connectionIndex[networkEntity.OwnerNetworkId] * TimeConfig.TicksPerSecond;
                    int index = (tick + TimeConfig.TicksPerSecond) % TimeConfig.TicksPerSecond;
                    var element = inputs[offset + index];

                    if (element.Tick >= tick)
                    {
                        var playerBaseLine = chunkPlayerBaseLines[i];
                        playerBaseLine.BaseLine = element.BaseLine;
                        playerBaseLine.Version = element.Version;
                        chunkPlayerBaseLines[i] = playerBaseLine;
                    }
                    
                    if (element.Tick == tick)
                    {
                        //<template:input>
                        //chunk##TYPE##[i] = element.##TYPE##;
                        //</template>
//<generated>
                        chunkMovementInput[i] = element.MovementInput;
                        chunkWeaponInput[i] = element.WeaponInput;
//</generated>
                        processedInputs.Add(new ProcessedInput()
                        {
                            ArrivedTime = element.ArrivedTime,
                            HasInput = true
                        });
                    }
                    else
                    {
                        processedInputs.Add(new ProcessedInput()
                        {
                            HasInput = false
                        });
                    }
                }
            }
        }
    }
}