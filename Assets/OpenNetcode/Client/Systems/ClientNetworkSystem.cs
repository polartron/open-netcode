using System;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Error;
using Unity.Networking.Transport.Utilities;
using UnityEngine;

namespace OpenNetcode.Client.Systems
{
    public interface IClientNetworkSystem
    {
        public NativeMultiHashMap<int, PacketArrayWrapper> ReceivedPackets { get; }
        public bool Connected { get; }

        public void Send(PacketArrayWrapper packet);
        
        public void SendUpdate();
        public void ReceiveUpdate();
        public void Connect(string address, ushort port, Action<bool> onConnected = default);
    }
    
    [DisableAutoCreation]
    public partial class ClientNetworkSystem : SystemBase, IClientNetworkSystem
    {
        public NativeMultiHashMap<int, PacketArrayWrapper> ReceivedPackets { get; private set; }
        public NativeList<PacketArrayWrapper> SendPackets { get; private set; }
        public bool Connected { get; private set; }
        
        private NetworkDriver _driver;
        private NativeArray<byte> _done;
        private NativeArray<NetworkConnection> _connection;
        
        protected override void OnCreate()
        {
            SendPackets = new NativeList<PacketArrayWrapper>(100, Allocator.Persistent);
            ReceivedPackets = new NativeMultiHashMap<int, PacketArrayWrapper>(100, Allocator.Persistent);
            
            var clientSettings = new NetworkSettings();
            clientSettings
                .WithNetworkConfigParameters(disconnectTimeoutMS: 90 * 1000, fixedFrameTimeMS: 0)
                .WithFragmentationStageParameters(payloadCapacity: 50 * 1024);

            _driver = NetworkDriver.Create(clientSettings);
            _driver.CreatePipeline(typeof(FragmentationPipelineStage));
            _connection = new NativeArray<NetworkConnection>(1, Allocator.Persistent);
            _done = new NativeArray<byte>(1, Allocator.Persistent);
            
        }

        protected override void OnDestroy()
        {
            _driver.Disconnect(_connection[0]);
            _connection.Dispose();
            _driver.Dispose();
            _done.Dispose();
            SendPackets.Dispose();

            ReceivedPackets.Dispose();
        }
        
        public void Connect(string address, ushort port, Action<bool> onConnected = default)
        {
            var endpoint = NetworkEndPoint.LoopbackIpv4;
            endpoint.Port = port;
            _connection[0] = _driver.Connect(endpoint);
            onConnected?.Invoke(_connection[0] != default(NetworkConnection));
        }

        protected override void OnUpdate()
        {
            
        }

        public void Send(PacketArrayWrapper packet)
        {
            SendPackets.Add(packet);
        }

        public void SendUpdate()
        {
            Dependency.Complete();
            
            if (!_connection[0].IsCreated)
                return;

            SendPacketsJob job = new SendPacketsJob()
            {
                Driver = _driver,
                Connection = _connection,
                Packets = SendPackets
            };

            Dependency = job.Schedule(Dependency);
            Dependency = _driver.ScheduleFlushSend(Dependency);
            Dependency.Complete();

            SendPackets.Clear();
        }

        public void ReceiveUpdate()
        {
            Dependency.Complete();

            if (_connection[0] == default(NetworkConnection))
            {
                //Not connected
                return;
            }

            Dependency = _driver.ScheduleUpdate(Dependency);
            Dependency.Complete();
            
            if (!_connection[0].IsCreated)
            {
                if (_done[0] != 1)
                {
                    Debug.Log("Something went wrong during connect");
                }
                
                return;
            }
            
            ReceivedPackets.Clear();

            DataStreamReader reader;
            NetworkEvent.Type cmd;

            while ((cmd = _connection[0].PopEvent(_driver, out reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Connect)
                {
                    Debug.Log("We are now connected to the server");
                    Connected = true;
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client got disconnected from server");
                    Connected = false;
                    _connection[0] = default(NetworkConnection);
                }
                else if (cmd == NetworkEvent.Type.Data)
                {
                    var data = new NativeArray<byte>(reader.Length, Allocator.Temp);
                    var type = Packets.ReadPacketType(ref reader);
                    reader.SeekSet(0);
                    reader.ReadBytes(data);

                    unsafe
                    {
                        ReceivedPackets.Add((int) type, new PacketArrayWrapper()
                        {
                            Pointer = data.GetUnsafePtr(),
                            Length = data.Length,
                            Allocator = Allocator.Temp
                        });
                    }
                }
            }
        }
    }
    
    [BurstCompile]
    struct SendPacketsJob : IJob
    {
        public NetworkDriver Driver;
        public NativeArray<NetworkConnection> Connection;
        public NativeList<PacketArrayWrapper> Packets;
        
        public void Execute()
        {
            if (Packets.Length > 0)
            {
                for (int i = 0; i < Packets.Length; i++)
                {
                    var packet = Packets[i];
                    DataStreamWriter stream;
                        
                    if (Driver.BeginSend(Connection[0], out stream) == (int) StatusCode.Success)
                    {
                        stream.WriteBytes(packet.GetArray<byte>());
                            
                        if (stream.HasFailedWrites)
                        {
                            Driver.AbortSend(stream);
                        }
                        else
                        {
                            Driver.EndSend(stream);
                        }
                    }
                }
            }

        }
    }
}
