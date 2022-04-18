using System;
using Unity.Collections;
using Unity.Entities;

namespace OpenNetcode.Shared.Components
{
    public struct NetworkedPrefabIndex : IComponentData
    {
        public int Value;
    }
}
