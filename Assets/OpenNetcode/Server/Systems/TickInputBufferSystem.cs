using OpenNetcode.Server.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using OpenNetcode.Shared.Utils;
using Shared.Time;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;

namespace OpenNetcode.Server.Systems
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickServerReceiveSystem))]
    public class TickInputBufferSystem<T> : SystemBase where T : unmanaged, INetworkedComponent, IComponentData
    {
        public struct InputBufferData
        {
            public InputMessage<T> Input;
            public double ArrivedTime;
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
                ComponentType.ReadOnly<T>(),
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
            var inputMessages = new NativeMultiHashMap<int, InputMessage<T>>(100, Allocator.Temp);
            
            if(packets.TryGetFirstValue((int) PacketType.Input, out PacketArrayWrapper wrapper, out NativeMultiHashMapIterator<int> iterator))
            {
                do
                {
                    var array = wrapper.GetArray<byte>();
                    var reader = new DataStreamReader(array);
                    InputMessage<T>.Read(ref inputMessages, ref reader, _compressionModel, wrapper.InternalId);
                } while (packets.TryGetNextValue(out wrapper, ref iterator));
            }
            
            foreach (var inputMessage in inputMessages)
            {
                if (!_connectionIndex.ContainsKey(inputMessage.Key))
                {
                    _connectionIndex[inputMessage.Key] = _freeIndexes[0];
                    _freeIndexes.RemoveAt(0);
                }

                int offset = _connectionIndex[inputMessage.Key] * TimeConfig.TicksPerSecond;
                int index = (inputMessage.Value.Tick + TimeConfig.TicksPerSecond) % TimeConfig.TicksPerSecond;
            
                if (_inputs[offset + index].Input.Tick != inputMessage.Value.Tick)
                {
                    _inputs[offset + index] = new InputBufferData()
                    {
                        Input = inputMessage.Value,
                        ArrivedTime = Time.ElapsedTime
                    };
                }
            }
            
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
                PlayerInputTypeHandle = GetComponentTypeHandle<T>(),
                PlayerBaseLineTypeHandle = GetComponentTypeHandle<PlayerBaseLine>()
            };
            
            return job.ScheduleParallel(_playersQuery, 4, Dependency);
        }
        
        [BurstCompile]
        public struct UpdatePlayerInputJob : IJobEntityBatch
        {
            [ReadOnly] public NativeArray<InputBufferData> inputs;
            [ReadOnly] public NativeHashMap<int, int> connectionIndex;
            [ReadOnly] public int tick;
            
            public ComponentTypeHandle<T> PlayerInputTypeHandle;
            public ComponentTypeHandle<PlayerBaseLine> PlayerBaseLineTypeHandle;
            public BufferTypeHandle<ProcessedInput> ProcessedInputTypeHandle;
            [ReadOnly] public ComponentTypeHandle<ServerNetworkedEntity> NetworkEntityTypeHandle;

            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                var chunkPlayerInputs = batchInChunk.GetNativeArray(PlayerInputTypeHandle);
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
                    
                    if (element.Input.Tick == tick)
                    {
                        chunkPlayerInputs[i] = element.Input.Input;
                        var playerBaseLine = chunkPlayerBaseLines[i];
                        playerBaseLine.BaseLine = element.Input.LastReceivedSnapshotTick;
                        playerBaseLine.Version = element.Input.Version;
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