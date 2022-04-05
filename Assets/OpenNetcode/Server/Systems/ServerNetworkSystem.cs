using OpenNetcode.Shared;
using OpenNetcode.Shared.Utils;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

namespace OpenNetcode.Server.Systems
{

    public interface IServerNetworkSystem
    {
        public NativeMultiHashMap<int, IncomingPacket> ReceivePackets { get; }
        public NativeMultiHashMap<int, PacketArrayWrapper> SendPackets { get; }
        public void Send(int networkId, PacketArrayWrapper wrapper);
        public void SendUpdate();
        public void ReceiveUpdate();
    }
    
    [DisableAutoCreation]
    public partial class ServerNetworkSystem : SystemBase, IServerNetworkSystem
    {
        public NativeMultiHashMap<int, IncomingPacket> ReceivePackets { get; private set; }
        public NativeMultiHashMap<int, PacketArrayWrapper> SendPackets { get; private set; }
        
        private NetworkDriver _driver;
        private NativeList<NetworkConnection> _connections;
        private NetworkPipeline _pipeline;

        protected override void OnUpdate()
        {
            
        }

        protected override void OnCreate()
        {
            ReceivePackets = new NativeMultiHashMap<int, IncomingPacket>(10000, Allocator.Persistent);
            SendPackets = new NativeMultiHashMap<int, PacketArrayWrapper>(10000, Allocator.Persistent);
            _connections = new NativeList<NetworkConnection>(10000, Allocator.Persistent);

            var serverSettings = new NetworkSettings();
            
            serverSettings
                .WithNetworkConfigParameters(disconnectTimeoutMS: 90 * 1000, fixedFrameTimeMS: 0)
                .WithFragmentationStageParameters(payloadCapacity: 50 * 1024);
            
            _driver = NetworkDriver.Create(serverSettings);
            _pipeline = _driver.CreatePipeline(typeof(FragmentationPipelineStage));

            ushort port = 27015;
            
            for (; port < 27020; port++)
            {
                var endpoint = NetworkEndPoint.AnyIpv4;
                endpoint.Port = port;

                if (_driver.Bind(endpoint) == 0)
                {
                    break;
                }
                else
                {
                    Debug.LogWarning($"Failed to bind to port {endpoint.Port}");
                }
            }
            
            if (!_driver.Bound)
            {
                Debug.LogError($"Failed to bind to ports.");
            }
            else
            {
                _driver.Listen();
                Debug.Log($"Listening on port {port}");
            }
            
            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            foreach (var connection in _connections)
            {
                _driver.Disconnect(connection);
            }
        
            _connections.Dispose();
            _driver.Dispose();
            SendPackets.Dispose();
            ReceivePackets.Dispose();
            base.OnDestroy();
        }

        public void Send(int networkId, PacketArrayWrapper wrapper)
        {
            SendPackets.Add(networkId, wrapper);
        }

        public void SendUpdate()
        {
            SendPacketsJob sendPacketsJob = new SendPacketsJob()
            {
                Connections = _connections,
                Driver = _driver.ToConcurrent(),
                Packets = SendPackets,
                Pipeline = _pipeline
            };

            Dependency = sendPacketsJob.Schedule(_connections, 4, Dependency);
            Dependency.Complete();
            SendPackets.Clear();
        }

        public void ReceiveUpdate()
        {
            Dependency.Complete();
            
            ReceivePackets.Clear();
            
            var connectionJob = new ServerUpdateConnectionsJob
            {
                Driver = _driver,
                Connections = _connections
            };
        
            var parsePacketsJob = new ParsePacketsJob()
            {
                Connections = _connections,
                Driver = _driver.ToConcurrent(),
                ReceivedMessages = ReceivePackets.AsParallelWriter()
            };

            Dependency = _driver.ScheduleUpdate();
            Dependency = connectionJob.Schedule(Dependency);
            Dependency = parsePacketsJob.Schedule(_connections, 4, Dependency);
            Dependency.Complete();
        }
        
        [BurstCompile]
        unsafe struct ParsePacketsJob : IJobParallelForDefer
        {
            public NetworkDriver.Concurrent Driver;
            [ReadOnly] public NativeList<NetworkConnection> Connections;
            public NativeMultiHashMap<int, IncomingPacket>.ParallelWriter ReceivedMessages;

            public void Execute(int index)
            {
                if (!Connections[index].IsCreated)
                {
                    Debug.LogError($"Connection {index} not created");
                    return;
                }

                NetworkEvent.Type cmd;
                
                while ((cmd = Driver.PopEventForConnection(Connections[index], out DataStreamReader reader)) != NetworkEvent.Type.Empty)
                {
                    if (cmd == NetworkEvent.Type.Data)
                    {
                        PacketType type = Packets.ReadPacketType(ref reader);
                        reader.SeekSet(0);
                        
                        ReceivedMessages.Add((int) type, new IncomingPacket()
                        {
                            Reader = reader,
                            Connection = Connections[index]
                        });
                    }
                    else if (cmd == NetworkEvent.Type.Disconnect)
                    {
                        Debug.Log("Client disconnected from server");
                        Connections[index] = default(NetworkConnection);
                    }
                }
            }
        }
    
        [BurstCompile]
        struct ServerUpdateConnectionsJob : IJob
        {
            public NetworkDriver Driver;
            public NativeList<NetworkConnection> Connections;

            public void Execute()
            {
                // Remove disconnected connections
                for (int i = 0; i < Connections.Length; i++)
                {
                    if (!Connections[i].IsCreated)
                    {
                        Connections.RemoveAtSwapBack(i);
                        --i;
                    }
                }
            
                // Accept new connections
                NetworkConnection c;
                while ((c = Driver.Accept()) != default(NetworkConnection))
                {
                    Connections.Add(c);
                    Debug.Log($"<color=red>Client connected with ID = {c.InternalId}</color>");
                }
            }
        }
        
        [BurstCompile]
        private struct SendPacketsJob : IJobParallelForDefer
        {
            [ReadOnly] public NativeMultiHashMap<int, PacketArrayWrapper> Packets;
            public NetworkDriver.Concurrent Driver;
            public NativeArray<NetworkConnection> Connections;
            public NetworkPipeline Pipeline;
        
            public void Execute(int index)
            {
                var connection = Connections[index];
                int id = connection.InternalId;

                if (Packets.ContainsKey(id))
                {
                    PacketArrayWrapper wrapper;
                    NativeMultiHashMapIterator<int> iterator;

                    if (Packets.CountValuesForKey(id) < 0)
                        return;
                
                    if(Packets.TryGetFirstValue(id, out wrapper, out iterator))
                    {
                        do
                        {
                            int errorCode = Driver.BeginSend(Pipeline, connection, out DataStreamWriter writer);

                            if (errorCode != (int) StatusCode.Success)
                            {
                                Driver.AbortSend(writer);
                                return;
                            }
                            
                            var array = wrapper.GetArray<byte>();
                            writer.WriteBytes(array);
                            Driver.EndSend(writer);

                        } while (Packets.TryGetNextValue(out wrapper, ref iterator));
                    }
                }
            }
        }
    }
}
