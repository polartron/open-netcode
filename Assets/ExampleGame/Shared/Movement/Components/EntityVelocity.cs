﻿using OpenNetcode.Shared.Attributes;
using Shared.Coordinates;
using Unity.Collections;
using Unity.Entities;

namespace ExampleGame.Shared.Movement.Components
{

    public enum CustomEnum
    {
        One,
        Two
    }
    [GenerateAuthoringComponent]
    [PublicSnapshot]
    public partial struct EntityVelocity : IComponentData
    {
        public GameUnits Value;
        public CustomEnum CustomEnum;
    }
}