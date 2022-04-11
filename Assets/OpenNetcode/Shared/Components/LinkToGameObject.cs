using Unity.Entities;
using UnityEngine;

namespace OpenNetcode.Shared.Components
{
    public struct LinkToGameObject : IComponentData
    {
        public int Type;
    }
}
