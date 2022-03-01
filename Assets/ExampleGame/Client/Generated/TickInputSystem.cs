using OpenNetcode.Client.Components;
using OpenNetcode.Client.Systems;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Systems;
using OpenNetcode.Shared.Time;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

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
        private NativeArray<MovementInput> _movementInputHistory = new NativeArray<MovementInput>(5, Allocator.Persistent);
        private NativeArray<WeaponInput> _weaponInputHistory = new NativeArray<WeaponInput>(5, Allocator.Persistent);
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
            _weaponInputHistory.Dispose();
//</generated>
            
            base.OnDestroy();
        }

        private bool WriteInput(int tick, in ClientData clientData, in Entity entity, ref DataStreamWriter writer)
        {
            int inputsLength = 5;

            //<template:input>
            //var current##TYPE## = EntityManager.GetComponentData<##TYPE##>(entity);
            //_##TYPELOWER##History[tick % inputsLength] = current##TYPE##;
            //var saved##TYPE## = EntityManager.GetBuffer<SavedInput<##TYPE##>>(entity);
            //saved##TYPE##.Add(new SavedInput<##TYPE##>()
            //{
            //    Value = current##TYPE##
            //});
            //
            //NativeArray<##TYPE##> ##TYPELOWER##Sorted = new NativeArray<##TYPE##>(inputsLength, Allocator.Temp);
            //
            //for (int i = 0; i < inputsLength; i++)
            //{
            //    int index = (tick - i + inputsLength) % inputsLength;
            //    ##TYPELOWER##Sorted[i] = _##TYPELOWER##History[index];
            //}
            //</template>
//<generated>
            var currentMovementInput = EntityManager.GetComponentData<MovementInput>(entity);
            _movementInputHistory[tick % inputsLength] = currentMovementInput;
            var savedMovementInput = EntityManager.GetBuffer<SavedInput<MovementInput>>(entity);
            savedMovementInput.Add(new SavedInput<MovementInput>()
            {
                Value = currentMovementInput
            });
            
            NativeArray<MovementInput> movementInputSorted = new NativeArray<MovementInput>(inputsLength, Allocator.Temp);
            
            for (int i = 0; i < inputsLength; i++)
            {
                int index = (tick - i + inputsLength) % inputsLength;
                movementInputSorted[i] = _movementInputHistory[index];
            }
            var currentWeaponInput = EntityManager.GetComponentData<WeaponInput>(entity);
            _weaponInputHistory[tick % inputsLength] = currentWeaponInput;
            var savedWeaponInput = EntityManager.GetBuffer<SavedInput<WeaponInput>>(entity);
            savedWeaponInput.Add(new SavedInput<WeaponInput>()
            {
                Value = currentWeaponInput
            });
            
            NativeArray<WeaponInput> weaponInputSorted = new NativeArray<WeaponInput>(inputsLength, Allocator.Temp);
            
            for (int i = 0; i < inputsLength; i++)
            {
                int index = (tick - i + inputsLength) % inputsLength;
                weaponInputSorted[i] = _weaponInputHistory[index];
            }
//</generated>
            
            
            Packets.WritePacketType(PacketType.Input, ref writer);
            writer.WriteRawBits((uint) inputsLength, 3);
            writer.WritePackedUInt((uint) tick, _compressionModel);
            writer.WritePackedUInt((uint) clientData.LastReceivedSnapshotIndex, _compressionModel);
            writer.WritePackedUInt((uint) clientData.Version, _compressionModel);

            //<template:input>
            //##TYPE## last##TYPE## = new ##TYPE##();
            //</template>
//<generated>
            MovementInput lastMovementInput = new MovementInput();
            WeaponInput lastWeaponInput = new WeaponInput();
//</generated>

            for (int i = 0; i < inputsLength; i++)
            {
                //<template:input>
                //var ##TYPELOWER## = ##TYPELOWER##Sorted[i];
                //##TYPELOWER##.WriteSnapshot(ref writer, _compressionModel, last##TYPE##);
                //last##TYPE## = ##TYPELOWER##;
                //</template>
//<generated>
                var movementInput = movementInputSorted[i];
                movementInput.WriteSnapshot(ref writer, _compressionModel, lastMovementInput);
                lastMovementInput = movementInput;
                var weaponInput = weaponInputSorted[i];
                weaponInput.WriteSnapshot(ref writer, _compressionModel, lastWeaponInput);
                lastWeaponInput = weaponInput;
//</generated>
            }
            
            return !writer.HasFailedWrites;
        }

        private void CompressInput(in Entity entity)
        {
            //<template:input>
            //{
            //    DataStreamWriter writer = new DataStreamWriter(10, Allocator.Temp);
            //    var input = EntityManager.GetComponentData<##TYPE##>(entity);
            //    input.WriteSnapshot(ref writer, _compressionModel, default);
            //    DataStreamReader reader = new DataStreamReader(writer.AsNativeArray());
            //    input.ReadSnapshot(ref reader, _compressionModel, default);
            //    EntityManager.SetComponentData(entity, input);
            //}
            //</template>
//<generated>
            {
                DataStreamWriter writer = new DataStreamWriter(10, Allocator.Temp);
                var input = EntityManager.GetComponentData<MovementInput>(entity);
                input.WriteSnapshot(ref writer, _compressionModel, default);
                DataStreamReader reader = new DataStreamReader(writer.AsNativeArray());
                input.ReadSnapshot(ref reader, _compressionModel, default);
                EntityManager.SetComponentData(entity, input);
            }
            {
                DataStreamWriter writer = new DataStreamWriter(10, Allocator.Temp);
                var input = EntityManager.GetComponentData<WeaponInput>(entity);
                input.WriteSnapshot(ref writer, _compressionModel, default);
                DataStreamReader reader = new DataStreamReader(writer.AsNativeArray());
                input.ReadSnapshot(ref reader, _compressionModel, default);
                EntityManager.SetComponentData(entity, input);
            }
//</generated>
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