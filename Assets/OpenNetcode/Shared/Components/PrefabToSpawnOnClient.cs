using Unity.Collections;
using Unity.Entities;

namespace OpenNetcode.Shared.Components
{
    public struct PrefabToSpawnOnClient : IComponentData
    {
        public FixedString64Bytes Value;
    }
}
