using OpenNetcode.Client.Components;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Messages;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    [DisableAutoCreation]
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup))]
    public class TickInputSystem : SystemBase
    {
        private IClientNetworkSystem _clientNetworkSystem;
        private TickReceiveResultSystem _tickReceiveResultSystem;
        private NetworkCompressionModel _compressionModel;

        //<template:input>
        //private NativeArray<##TYPE##> _##TYPELOWER##History = new NativeArray<##TYPE##>(5, Allocator.Persistent);
        //</template>
//<generated>
        private NativeArray<CharacterInput> _characterInputHistory = new NativeArray<CharacterInput>(5, Allocator.Persistent);
//</generated>

        public TickInputSystem(IClientNetworkSystem clientNetworkSystem)
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
            //<template:input>
            //_##TYPELOWER##History.Dispose();
            //</template>
//<generated>
            _characterInputHistory.Dispose();
//</generated>
            
            base.OnDestroy();
        }

        private void AddInputPacket<T>(int tick, in ClientData clientData, in Entity entity, ref DataStreamWriter writer, ref NativeArray<T> history) where T : unmanaged, INetworkedComponent
        {
            T input = EntityManager.GetComponentData<T>(entity);

            int length = history.Length;
            
            history[tick % length] = input;

            NativeArray<T> sorted = new NativeArray<T>(length, Allocator.Temp);

            for (int i = 0; i < length; i++)
            {
                int index = (tick - i + length) % length;
                sorted[i] = history[index];
            }

            InputMessage<T>.Write(ref sorted, tick, clientData.LastReceivedSnapshotIndex, clientData.Version,  ref writer, _compressionModel);

            var savedInput = EntityManager.GetBuffer<SavedInput<T>>(entity);
            
            savedInput.Add(new SavedInput<T>()
            {
                Value = input
            });
        }

        private void CompressInput<T>(in Entity entity) where T : unmanaged, INetworkedComponent
        {
            DataStreamWriter writer = new DataStreamWriter(10, Allocator.Temp);
            T input = EntityManager.GetComponentData<T>(entity);
            input.Write(ref writer, _compressionModel);
            DataStreamReader reader = new DataStreamReader(writer.AsNativeArray());
            input.Read(ref reader, _compressionModel);
            EntityManager.SetComponentData(entity, input);
        }

        protected override void OnUpdate()
        {
            if (!_clientNetworkSystem.Connected)
            {
                return;
            }

            var tickData = GetSingleton<TickData>();
            var clientData = GetSingleton<ClientData>();
            Entity clientEntity = clientData.LocalPlayer;

            var writer = new DataStreamWriter(1000, Allocator.Temp);
            
            //<template:input>
            //CompressInput<##TYPE##>(clientEntity);
            //AddInputPacket<##TYPE##>(tickData.Value, clientData, clientEntity, ref writer, ref _##TYPELOWER##History);
            //</template>
//<generated>
            CompressInput<CharacterInput>(clientEntity);
            AddInputPacket<CharacterInput>(tickData.Value, clientData, clientEntity, ref writer, ref _characterInputHistory);
//</generated>
            
            _clientNetworkSystem.Send(Packets.WrapPacket(writer));
            _tickReceiveResultSystem.AddSentInput(GetSingleton<TickData>().Value, (float) Time.ElapsedTime);
        }
    }
}