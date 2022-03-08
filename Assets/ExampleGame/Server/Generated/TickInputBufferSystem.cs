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
using Unity.Jobs;

//</generated>

namespace Server.Generated
{
    public struct ReceivedMovementInput : IBufferElementData
    {
        public int Tick;
        public MovementInput Input;
    }

    public struct ReceivedWeaponInput : IBufferElementData
    {
        public int Tick;
        public WeaponInput Input;
    }
    
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickServerReceiveSystem))]
    [DisableAutoCreation]
    public class TickInputBufferSystem : SystemBase
    {
        private EntityQuery _playersQuery;
        private IServerNetworkSystem _server;
        private NetworkCompressionModel _compressionModel;

        public TickInputBufferSystem(IServerNetworkSystem server)
        {
            _server = server;
        }

        protected override void OnCreate()
        {
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

        protected override void OnUpdate()
        {
            ReadPlayerInputJob readPlayerInputJob = new ReadPlayerInputJob()
            {
                ElapsedTime = UnityEngine.Time.realtimeSinceStartupAsDouble,
                CompressionModel = _compressionModel,
                ReceivedPackets = _server.ReceivePackets,
                ServerNetworkedEntityTypeHandle = GetComponentTypeHandle<ServerNetworkedEntity>(true),
                PlayerBaseLineTypeHandle = GetComponentTypeHandle<PlayerBaseLine>(),
                InputTimeDataTypeHandle = GetComponentTypeHandle<InputTimeData>(),
                ReceivedMovementInput = GetBufferTypeHandle<ReceivedMovementInput>(),
                ReceivedWeaponInput = GetBufferTypeHandle<ReceivedWeaponInput>()
            };

            Dependency = readPlayerInputJob.ScheduleParallel(_playersQuery, 4, Dependency);
            Dependency.Complete();

            UpdatePlayerInputJob updatePlayerInputJob = new UpdatePlayerInputJob()
            {
                Tick = GetSingleton<TickData>().Value,
                MovementInputTypeHandle = GetComponentTypeHandle<MovementInput>(),
                WeaponInputTypeHandle = GetComponentTypeHandle<WeaponInput>(),
                ReceivedMovementInput = GetBufferTypeHandle<ReceivedMovementInput>(true),
                ReceivedWeaponInput = GetBufferTypeHandle<ReceivedWeaponInput>(true)
            };
            
            Dependency = updatePlayerInputJob.ScheduleParallel(_playersQuery, 4, Dependency);
            Dependency.Complete();
        }

        [BurstCompile]
        private struct ReadPlayerInputJob : IJobEntityBatch
        {
            [ReadOnly] public double ElapsedTime;
            
            [ReadOnly] public NetworkCompressionModel CompressionModel;
            [ReadOnly] public NativeMultiHashMap<int, PacketArrayWrapper> ReceivedPackets;
            [ReadOnly] public ComponentTypeHandle<ServerNetworkedEntity> ServerNetworkedEntityTypeHandle;

            public ComponentTypeHandle<PlayerBaseLine> PlayerBaseLineTypeHandle;
            public ComponentTypeHandle<InputTimeData> InputTimeDataTypeHandle;
            public BufferTypeHandle<ReceivedMovementInput> ReceivedMovementInput;
            public BufferTypeHandle<ReceivedWeaponInput> ReceivedWeaponInput;
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var serverNetworkedEntities = batchInChunk.GetNativeArray(ServerNetworkedEntityTypeHandle);
                var receivedMovementInputs = batchInChunk.GetBufferAccessor(ReceivedMovementInput);
                var receivedWeaponInputs = batchInChunk.GetBufferAccessor(ReceivedWeaponInput);
                var playerBaselines = batchInChunk.GetNativeArray(PlayerBaseLineTypeHandle);
                var inputTimeDatas = batchInChunk.GetNativeArray(InputTimeDataTypeHandle);

                for (int i = 0; i < serverNetworkedEntities.Length; i++)
                {
                    var serverNetworkedEntity = serverNetworkedEntities[i];
                    
                    if(ReceivedPackets.TryGetFirstValue((int) PacketType.Input, out PacketArrayWrapper wrapper, out NativeMultiHashMapIterator<int> iterator))
                    {
                        do
                        {
                            if (wrapper.InternalId != serverNetworkedEntity.OwnerNetworkId)
                                continue;

                            var receivedMovementInput = receivedMovementInputs[i];
                            var receivedWeaponInput = receivedWeaponInputs[i];
                            
                            var array = wrapper.GetArray<byte>();
                            var reader = new DataStreamReader(array);
                            
                            Packets.ReadPacketType(ref reader);
                            int count = (int) reader.ReadRawBits(3);
                            int tick = (int) reader.ReadPackedUInt(CompressionModel);
                            int lastReceivedSnapshotTick = (int) reader.ReadPackedUInt(CompressionModel);
                            int version = (int) reader.ReadPackedUInt(CompressionModel);

                            var playerBaseline = playerBaselines[i];
                            playerBaseline.Version = version;
                            playerBaseline.BaseLine = lastReceivedSnapshotTick;
                            playerBaselines[i] = playerBaseline;

                            inputTimeDatas[i] = new InputTimeData()
                            {
                                Tick = tick,
                                ArrivedTime = ElapsedTime
                            };
                            
                            //<template:input>
                            //##TYPE## last##TYPE## = new ##TYPE##();
                            //</template>
//<generated>
                            MovementInput lastMovementInput = new MovementInput();
                            WeaponInput lastWeaponInput = new WeaponInput();
//</generated>

                            for (int j = 0; j < count; j++)
                            {
                                //<template:input>
                                //##TYPE## ##TYPELOWER## = new ##TYPE##();
                                //##TYPELOWER##.ReadSnapshot(ref reader, _compressionModel, last##TYPE##);
                                //last##TYPE## = ##TYPELOWER##;
                                //</template>
//<generated>
                                MovementInput movementInput = new MovementInput();
                                movementInput.ReadSnapshot(ref reader, CompressionModel, lastMovementInput);
                                lastMovementInput = movementInput;

                                WeaponInput weaponInput = new WeaponInput();
                                weaponInput.ReadSnapshot(ref reader, CompressionModel, lastWeaponInput);
                                lastWeaponInput = weaponInput;
//</generated>

                                int index = (tick - j + TimeConfig.TicksPerSecond) % TimeConfig.TicksPerSecond;

                                if (receivedMovementInput[index].Tick != tick - j)
                                {
                                    receivedMovementInput[index] = new ReceivedMovementInput()
                                    {
                                        Tick = tick - j,
                                        Input = movementInput
                                    };
                                }

                                if (receivedWeaponInput[index].Tick != tick - j)
                                {
                                    receivedWeaponInput[index] = new ReceivedWeaponInput()
                                    {
                                        Tick = tick - j,
                                        Input = weaponInput
                                    };
                                }
                            }
                        } while (ReceivedPackets.TryGetNextValue(out wrapper, ref iterator));
                    }
                }
            }
        }

        [BurstCompile]
        private struct UpdatePlayerInputJob : IJobEntityBatch
        {
            [ReadOnly] public BufferTypeHandle<ReceivedMovementInput> ReceivedMovementInput;
            [ReadOnly] public BufferTypeHandle<ReceivedWeaponInput> ReceivedWeaponInput;
            [ReadOnly] public int Tick;
            
            public ComponentTypeHandle<MovementInput> MovementInputTypeHandle;
            public ComponentTypeHandle<WeaponInput> WeaponInputTypeHandle;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var movementInputs = batchInChunk.GetNativeArray(MovementInputTypeHandle);
                var weaponInputs = batchInChunk.GetNativeArray(WeaponInputTypeHandle);

                var receivedMovementInputs = batchInChunk.GetBufferAccessor(ReceivedMovementInput);
                var receivedWeaponInputs = batchInChunk.GetBufferAccessor(ReceivedWeaponInput);
                

                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var receivedMovementInput = receivedMovementInputs[i];
                    var receivedWeaponInput = receivedWeaponInputs[i];
                    
                    int index = (Tick + TimeConfig.TicksPerSecond) % TimeConfig.TicksPerSecond;
                    var movementInput = receivedMovementInput[index];
                    var weaponInput = receivedWeaponInput[index];

                    if (movementInput.Tick == Tick)
                    {
                        movementInputs[i] = movementInput.Input;
                    }
                    
                    if (weaponInput.Tick == Tick)
                    {
                        weaponInputs[i] = weaponInput.Input;
                    }
                }
            }
        }
    }
}