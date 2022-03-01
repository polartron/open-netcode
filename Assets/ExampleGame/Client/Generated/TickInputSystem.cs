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
using Unity.Burst;
using UnityEngine;

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
        private NativeArray<MovementInput> _movementInputHistory = new NativeArray<MovementInput>(5, Allocator.Persistent);
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
            _movementInputHistory.Dispose();
//</generated>
            
            base.OnDestroy();
        }

        private bool WriteInput(int tick, in ClientData clientData, in Entity entity, ref DataStreamWriter writer)
        {
            MovementInput currentMovementInput = EntityManager.GetComponentData<MovementInput>(entity);
            int length = _movementInputHistory.Length;
            _movementInputHistory[tick % length] = currentMovementInput;
            var savedMovementInput = EntityManager.GetBuffer<SavedInput<MovementInput>>(entity);
            savedMovementInput.Add(new SavedInput<MovementInput>()
            {
                Value = currentMovementInput
            });

            NativeArray<MovementInput> movementInputSorted = new NativeArray<MovementInput>(length, Allocator.Temp);

            for (int i = 0; i < length; i++)
            {
                int index = (tick - i + length) % length;
                movementInputSorted[i] = _movementInputHistory[index];
            }
            
            Packets.WritePacketType(PacketType.Input, ref writer);
            writer.WriteRawBits((uint) movementInputSorted.Length, 3);
            writer.WritePackedUInt((uint) tick, _compressionModel);
            writer.WritePackedUInt((uint) clientData.LastReceivedSnapshotIndex, _compressionModel);
            writer.WritePackedUInt((uint) clientData.Version, _compressionModel);

            MovementInput lastMovementInput = new MovementInput();

            for (int i = 0; i < movementInputSorted.Length; i++)
            {
                var input = movementInputSorted[i];
                input.WriteSnapshot(ref writer, _compressionModel, lastMovementInput);
                lastMovementInput = input;
            }
            
            return !writer.HasFailedWrites;
        }

        private void CompressInput(in Entity entity)
        {
            {
                DataStreamWriter writer = new DataStreamWriter(10, Allocator.Temp);
                MovementInput input = EntityManager.GetComponentData<MovementInput>(entity);
                input.WriteSnapshot(ref writer, _compressionModel, default);
                DataStreamReader reader = new DataStreamReader(writer.AsNativeArray());
                input.ReadSnapshot(ref reader, _compressionModel, default);
                EntityManager.SetComponentData(entity, input);
            }
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
            
            CompressInput(clientEntity);
            
            if (!WriteInput(tickData.Value, clientData, clientEntity, ref writer))
            {
                Debug.Log("Error");
            }
            
            _clientNetworkSystem.Send(Packets.WrapPacket(writer));
            _tickReceiveResultSystem.AddSentInput(GetSingleton<TickData>().Value, (float) Time.ElapsedTime);
        }
    }
}