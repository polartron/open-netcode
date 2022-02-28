using OpenNetcode.Server.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using OpenNetcode.Shared.Utils;
using OpenNetcode.Server.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;

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
            //public InputMessage<##TYPE##> ##TYPE##Message;
            //</template>
//<generated>
            public InputMessage<MovementInput> MovementInputMessage;
//</generated>
            public double ArrivedTime;
            public int Tick;
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
            //<template:input>
            //var ##TYPELOWER##Messages = new NativeMultiHashMap<int, InputMessage<##TYPE##>>(100, Allocator.Temp);
            //</template>
//<generated>
            var movementInputMessages = new NativeMultiHashMap<int, InputMessage<MovementInput>>(100, Allocator.Temp);
//</generated>
            
            if(packets.TryGetFirstValue((int) PacketType.Input, out PacketArrayWrapper wrapper, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    var array = wrapper.GetArray<byte>();
                    var reader = new DataStreamReader(array);
                    //<template:input>
                    //InputMessage<##TYPE##>.Read(ref ##TYPELOWER##Messages, ref reader, _compressionModel, wrapper.InternalId);
                    //</template>
//<generated>
                    InputMessage<MovementInput>.Read(ref movementInputMessages, ref reader, _compressionModel, wrapper.InternalId);
//</generated>
                } while (packets.TryGetNextValue(out wrapper, ref iterator));
            }

            
            //<template:input>
            //foreach (var inputMessage in ##TYPELOWER##Messages)
            //{
            //    if (!_connectionIndex.ContainsKey(inputMessage.Key))
            //    {
            //        _connectionIndex[inputMessage.Key] = _freeIndexes[0];
            //        _freeIndexes.RemoveAt(0);
            //    }
//
            //    int offset = _connectionIndex[inputMessage.Key] * TimeConfig.TicksPerSecond;
            //    int index = (inputMessage.Value.Tick + TimeConfig.TicksPerSecond) % TimeConfig.TicksPerSecond;
            //
            //    if (_inputs[offset + index].##TYPE##Message.Tick != inputMessage.Value.Tick)
            //    {
            //        var inputBufferData = _inputs[offset + index];
            //        inputBufferData.##TYPE##Message = inputMessage.Value;
            //        inputBufferData.ArrivedTime = Time.ElapsedTime;
            //        inputBufferData.Tick = inputMessage.Value.Tick;
            //        _inputs[offset + index] = inputBufferData;
            //    }
            //}
            //</template>
//<generated>
            foreach (var inputMessage in movementInputMessages)
            {
                if (!_connectionIndex.ContainsKey(inputMessage.Key))
                {
                    _connectionIndex[inputMessage.Key] = _freeIndexes[0];
                    _freeIndexes.RemoveAt(0);
                }

                int offset = _connectionIndex[inputMessage.Key] * TimeConfig.TicksPerSecond;
                int index = (inputMessage.Value.Tick + TimeConfig.TicksPerSecond) % TimeConfig.TicksPerSecond;
            
                if (_inputs[offset + index].MovementInputMessage.Tick != inputMessage.Value.Tick)
                {
                    var inputBufferData = _inputs[offset + index];
                    inputBufferData.MovementInputMessage = inputMessage.Value;
                    inputBufferData.ArrivedTime = Time.ElapsedTime;
                    inputBufferData.Tick = inputMessage.Value.Tick;
                    _inputs[offset + index] = inputBufferData;
                }
            }
//</generated>
            
            NativeHashMap<int, int> connectionIndex = _connectionIndex;
            NativeArray<InputBufferData> inputs = _inputs;
            Dependency = Schedule(inputs, connectionIndex, Dependency);
            Dependency.Complete();
        }
        
        private JobHandle Schedule(in NativeArray<InputBufferData> inputs, in NativeHashMap<int, int> connectionIndex, JobHandle dep = new JobHandle())
        {
            int tick = GetSingleton<TickData>().Value;
            
            UpdatePlayerInputJob job = new UpdatePlayerInputJob()
            {
                inputs = inputs,
                connectionIndex = connectionIndex,
                tick = tick,
                NetworkEntityTypeHandle = GetComponentTypeHandle<ServerNetworkedEntity>(true),
                ProcessedInputTypeHandle = GetBufferTypeHandle<ProcessedInput>(),
                PlayerBaseLineTypeHandle = GetComponentTypeHandle<PlayerBaseLine>(),
                //<template:input>
                //##TYPE##TypeHandle = GetComponentTypeHandle<##TYPE##>(),
                //</template>
//<generated>
                MovementInputTypeHandle = GetComponentTypeHandle<MovementInput>(),
//</generated>
            };
            
            return job.ScheduleParallel(_playersQuery, 4, Dependency);
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
                    
                    if (element.Tick == tick)
                    {
                        var playerBaseLine = chunkPlayerBaseLines[i];
                        //<template:input>
                        //chunk##TYPE##[i] = element.##TYPE##Message.Input;
                        //playerBaseLine.BaseLine = element.##TYPE##Message.LastReceivedSnapshotTick;
                        //playerBaseLine.Version = element.##TYPE##Message.Version;
                        //</template>
//<generated>
                        chunkMovementInput[i] = element.MovementInputMessage.Input;
                        playerBaseLine.BaseLine = element.MovementInputMessage.LastReceivedSnapshotTick;
                        playerBaseLine.Version = element.MovementInputMessage.Version;
//</generated>
                        chunkPlayerBaseLines[i] = playerBaseLine;

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