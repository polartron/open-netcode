using OpenNetcode.Client.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Debugging;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using SourceConsole;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace ExampleGame.Client.Systems
{
    [DisableAutoCreation]    
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickClientReceiveSystem))]
    public partial class TickDevClient : SystemBase
    {
        private IClientNetworkSystem _client;
        private bool _connecting = false;
        private bool _loggingIn = false;

        public TickDevClient(IClientNetworkSystem client)
        {
            _client = client;
        }

        public void TryConnect(string ip, ushort port)
        {
            if (!_client.Connected && !_connecting)
            {
                _connecting = true;
                _client.Connect(ip, port, b =>
                {
                    Debug.Log("Connected " + b);
                    _connecting = false;
                });
            }
        }

        [ConCommand("disconnect")]
        public static void Disconnect()
        {
            ClientBootstrap.World.GetExistingSystem<ClientNetworkSystem>().Disconnect();
        }
        
        [ConCommand("connect")]
        public static void Connect(string ipAndPort)
        {
            string[] split = ipAndPort.Split(':');
            if (split.Length < 2)
            {
                Debug.LogError($"Failed to get IP and Port from the string {ipAndPort}. Make sure it's in the correct format <ip>:<port>");
                return;
            }

            string ipString = split[0];
            string portString = split[1];
            if (!ushort.TryParse(portString, out ushort port))
            {
                Debug.LogError($"Failed to parse port from the string {portString}");
                return;
            }

            ClientBootstrap.World.GetExistingSystem<TickDevClient>().TryConnect(ipString, port);

        }

        protected override void OnUpdate()
        {
            if (!_loggingIn && _client.Connected)
            {
                _loggingIn = true;
                var writer = new DataStreamWriter(10, Allocator.Temp);
                new LoginMessage()
                {
                    Guest = true
                }.Write(ref writer);
                
                _client.Send(Packets.WrapPacket(writer));
            }
            
            DebugOverlay.AddTickElement("Client Tick", new TickElement()
            {
                Color = Color.green,
                Tick = GetSingleton<TickData>().Value
            });
        }
    }
}