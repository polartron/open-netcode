using Unity.Collections;
using Unity.Entities;

namespace OpenNetcode.Shared.Components
{
    public struct NetworkedPrefabGuid : IComponentData
    {
        public FixedString64Bytes Value;
    }
}
