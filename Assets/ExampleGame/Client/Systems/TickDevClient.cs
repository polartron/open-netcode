using OpenNetcode.Client.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace ExampleGame.Client.Systems
{
    [DisableAutoCreation]    
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    [UpdateAfter(typeof(TickClientReceiveSystem))]
    public class TickDevClient : SystemBase
    {
        private IClientNetworkSystem _client;
        private bool _connecting = false;
        private bool _loggingIn = false;

        public TickDevClient(IClientNetworkSystem client)
        {
            _client = client;
        }

        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Q) && !_client.Connected && !_connecting)
            {
                _connecting = true;
                _client.Connect("0.0.0.0", 27015, b =>
                {
                    Debug.Log("Connected " + b);
                    _connecting = false;
                });
            }

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
        }
    }
}