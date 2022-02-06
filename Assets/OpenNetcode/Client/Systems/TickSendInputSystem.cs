using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace OpenNetcode.Client.Systems
{
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    public class TickSendInputSystem<TPrediction, TInput> : SystemBase
        where TPrediction : unmanaged, INetworkedComponent, IComponentData
        where TInput : unmanaged, INetworkedComponent, IComponentData
    {
        private IClientNetworkSystem _clientNetworkSystem;
        private NativeArray<TInput> _inputs = new NativeArray<TInput>(5, Allocator.Persistent);
        private TickReceiveResultSystem _tickReceiveResultSystem;
        private NetworkCompressionModel _compressionModel;

        public TickSendInputSystem(IClientNetworkSystem clientNetworkSystem)
        {
            _clientNetworkSystem = clientNetworkSystem;
        }
        
        protected override void OnCreate()
        {
            _tickReceiveResultSystem = World.GetExistingSystem<TickReceiveResultSystem>();
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);

            base.OnCreate();
        }

        protected override void OnDestroy()
        {
            _inputs.Dispose();
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            if (!_clientNetworkSystem.Connected)
            {
                return;
            }

            var tickData = GetSingleton<TickData>();
            var clientData = GetSingleton<ClientData>();
            Entity localEntity = clientData.LocalPlayer;
            TInput input = EntityManager.GetComponentData<TInput>(localEntity);

            _inputs[tickData.Value % _inputs.Length] = input;

            NativeArray<TInput> sorted = new NativeArray<TInput>(_inputs.Length, Allocator.Temp);
            
            for (int i = 0; i < _inputs.Length; i++)
            {
                int index = (tickData.Value - i + _inputs.Length) % _inputs.Length;
                sorted[i] = _inputs[index];
            }

            if (Input.GetKey(KeyCode.Space))
            {
                clientData.LastReceivedSnapshotIndex = 0;
            }
            
            var writer = new DataStreamWriter(100, Allocator.Temp);
            InputMessage<TInput>.Write(ref sorted, tickData.Value, clientData.LastReceivedSnapshotIndex, clientData.Version,  ref writer,
                _compressionModel);
            
            _clientNetworkSystem.Send(Packets.WrapPacket(writer));
            _tickReceiveResultSystem.AddSentInput(GetSingleton<TickData>().Value, (float) Time.ElapsedTime);
        }
    }
}