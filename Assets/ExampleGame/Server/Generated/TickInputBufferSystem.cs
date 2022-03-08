using OpenNetcode.Server.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using OpenNetcode.Shared.Utils;
using OpenNetcode.Server.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    //<template:input>
    //public struct Received##TYPE## : IBufferElementData
    //{
    //    public int Tick;
    //    public ##TYPE## Input;
    //}
    //</template>
//<generated>
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
//</generated>
    
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
                //<template:input>
                //Received##TYPE## = GetBufferTypeHandle<Received##TYPE##>(),
                //</template>
//<generated>
                ReceivedMovementInput = GetBufferTypeHandle<ReceivedMovementInput>(),
                ReceivedWeaponInput = GetBufferTypeHandle<ReceivedWeaponInput>(),
//</generated>
            };

            Dependency = readPlayerInputJob.ScheduleParallel(_playersQuery, 4, Dependency);
            Dependency.Complete();

            UpdatePlayerInputJob updatePlayerInputJob = new UpdatePlayerInputJob()
            {
                Tick = GetSingleton<TickData>().Value,
                
                //<template:input>
                //##TYPE##TypeHandle = GetComponentTypeHandle<##TYPE##>(),
                //Received##TYPE##TypeHandle = GetBufferTypeHandle<Received##TYPE##>(true),
                //</template>
//<generated>
                MovementInputTypeHandle = GetComponentTypeHandle<MovementInput>(),
                ReceivedMovementInputTypeHandle = GetBufferTypeHandle<ReceivedMovementInput>(true),
                WeaponInputTypeHandle = GetComponentTypeHandle<WeaponInput>(),
                ReceivedWeaponInputTypeHandle = GetBufferTypeHandle<ReceivedWeaponInput>(true),
//</generated>
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
            //<template:input>
            //public BufferTypeHandle<Received##TYPE##> Received##TYPE##;
            //</template>
//<generated>
            public BufferTypeHandle<ReceivedMovementInput> ReceivedMovementInput;
            public BufferTypeHandle<ReceivedWeaponInput> ReceivedWeaponInput;
//</generated>
            
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var serverNetworkedEntities = batchInChunk.GetNativeArray(ServerNetworkedEntityTypeHandle);
                var playerBaselines = batchInChunk.GetNativeArray(PlayerBaseLineTypeHandle);
                var inputTimeDatas = batchInChunk.GetNativeArray(InputTimeDataTypeHandle);
                //<template:input>
                //var received##TYPE##s = batchInChunk.GetBufferAccessor(Received##TYPE##);
                //</template>
//<generated>
                var receivedMovementInputs = batchInChunk.GetBufferAccessor(ReceivedMovementInput);
                var receivedWeaponInputs = batchInChunk.GetBufferAccessor(ReceivedWeaponInput);
//</generated>

                for (int i = 0; i < serverNetworkedEntities.Length; i++)
                {
                    var serverNetworkedEntity = serverNetworkedEntities[i];
                    
                    if(ReceivedPackets.TryGetFirstValue((int) PacketType.Input, out PacketArrayWrapper wrapper, out NativeMultiHashMapIterator<int> iterator))
                    {
                        do
                        {
                            if (wrapper.InternalId != serverNetworkedEntity.OwnerNetworkId)
                                continue;

                            //<template:input>
                            //var received##TYPE## = received##TYPE##s[i];
                            //</template>
//<generated>
                            var receivedMovementInput = receivedMovementInputs[i];
                            var receivedWeaponInput = receivedWeaponInputs[i];
//</generated>
                            
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
                                //##TYPELOWER##.ReadSnapshot(ref reader, CompressionModel, last##TYPE##);
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

                                //<template:input>
                                //if (received##TYPE##[index].Tick != tick - j)
                                //{
                                //    received##TYPE##[index] = new Received##TYPE##()
                                //    {
                                //        Tick = tick - j,
                                //        Input = ##TYPELOWER##
                                //    };
                                //}
                                //</template>
//<generated>
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
//</generated>
                            }
                        } while (ReceivedPackets.TryGetNextValue(out wrapper, ref iterator));
                    }
                }
            }
        }

        [BurstCompile]
        private struct UpdatePlayerInputJob : IJobEntityBatch
        {
            [ReadOnly] public int Tick;
            
            //<template:input>
            //[ReadOnly] public BufferTypeHandle<Received##TYPE##> Received##TYPE##TypeHandle;
            //public ComponentTypeHandle<##TYPE##> ##TYPE##TypeHandle;
            //</template>
//<generated>
            [ReadOnly] public BufferTypeHandle<ReceivedMovementInput> ReceivedMovementInputTypeHandle;
            public ComponentTypeHandle<MovementInput> MovementInputTypeHandle;
            [ReadOnly] public BufferTypeHandle<ReceivedWeaponInput> ReceivedWeaponInputTypeHandle;
            public ComponentTypeHandle<WeaponInput> WeaponInputTypeHandle;
//</generated>

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                //<template:input>
                //var ##TYPELOWER##s = batchInChunk.GetNativeArray(##TYPE##TypeHandle);
                //var received##TYPE##s = batchInChunk.GetBufferAccessor(Received##TYPE##TypeHandle);
                //</template>
//<generated>
                var movementInputs = batchInChunk.GetNativeArray(MovementInputTypeHandle);
                var receivedMovementInputs = batchInChunk.GetBufferAccessor(ReceivedMovementInputTypeHandle);
                var weaponInputs = batchInChunk.GetNativeArray(WeaponInputTypeHandle);
                var receivedWeaponInputs = batchInChunk.GetBufferAccessor(ReceivedWeaponInputTypeHandle);
//</generated>
                

                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    int index = (Tick + TimeConfig.TicksPerSecond) % TimeConfig.TicksPerSecond;

                    //<template:input>
                    //var received##TYPE## = received##TYPE##s[i];
                    //var ##TYPELOWER## = received##TYPE##[index];
                    //if (##TYPELOWER##.Tick == Tick)
                    //{
                    //    ##TYPELOWER##s[i] = ##TYPELOWER##.Input;
                    //}
                    //</template>
//<generated>
                    var receivedMovementInput = receivedMovementInputs[i];
                    var movementInput = receivedMovementInput[index];
                    if (movementInput.Tick == Tick)
                    {
                        movementInputs[i] = movementInput.Input;
                    }
                    var receivedWeaponInput = receivedWeaponInputs[i];
                    var weaponInput = receivedWeaponInput[index];
                    if (weaponInput.Tick == Tick)
                    {
                        weaponInputs[i] = weaponInput.Input;
                    }
//</generated>
                }
            }
        }
    }
}