using OpenNetcode.Movement.Components;
using OpenNetcode.Shared.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Networking.Transport;
using UnityEngine;

namespace ExampleGame.Client.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TickSystem))]
    [DisableAutoCreation]
    public class PlayerInputSystem : SystemBase
    {
        private NetworkCompressionModel _compressionModel;

        protected override void OnCreate()
        {
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            Vector2 input = Vector3.zero;
            Vector2 move = new Vector2(input.x, input.y);
                
            if (Input.GetKey(KeyCode.W))
            {
                move.y += 1;
            }
            
            if (Input.GetKey(KeyCode.S))
            {
                move.y -= 1;
            }
            
            if (Input.GetKey(KeyCode.D))
            {
                move.x += 1;
            }
            
            if (Input.GetKey(KeyCode.A))
            {
                move.x -= 1;
            }

            // Use input as if it's been received by the server.
            // If we don't do this the prediction will be wrong because
            // the server works with compressed input.
                
            DataStreamWriter writer = new DataStreamWriter(10, Allocator.Temp);
            CharacterInput predictedInput = new CharacterInput()
            {
                Move = move
            };
                
            predictedInput.Write(ref writer, _compressionModel);
            DataStreamReader reader = new DataStreamReader(writer.AsNativeArray());
            predictedInput.Read(ref reader, _compressionModel);

            var clientData = GetSingleton<ClientData>();
            
            EntityManager.SetComponentData(clientData.LocalPlayer, predictedInput);
        }
    }
}
