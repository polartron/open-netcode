using System;
using OpenNetcode.Shared.Attributes;
using OpenNetcode.Shared.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Networking.Transport;
using UnityEngine;

namespace ExampleGame.Shared.Movement.Components
{
    [GenerateAuthoringComponent]
    [NetworkedInput]
    public partial struct MovementInput : IComponentData
    {
        public float2 Move;
        public float Rotation;
    }
}
