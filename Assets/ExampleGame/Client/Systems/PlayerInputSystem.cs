using ExampleGame.Shared.Movement.Components;
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
        protected override void OnUpdate()
        {
            Vector2 input = Vector3.zero; // Get joystick movement
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
            
            var clientData = GetSingleton<ClientData>();
            
            MovementInput predictedInput = new MovementInput()
            {
                Move = move
            };
            
            EntityManager.SetComponentData(clientData.LocalPlayer, predictedInput);
        }
    }
}
