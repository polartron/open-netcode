using System;
using Unity.Entities;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    internal struct ServerEntitySnapshot : IComparable<ServerEntitySnapshot>
    {
        public Entity Entity;
        public int ComponentMask;
        public int EventMask;
        public int PrefabType;

        //<template:publicsnapshot>
        //public int ##TYPE##Index;
        //</template>
//<generated>
        public int EntityVelocityIndex;
        public int PathComponentIndex;
        public int EntityPositionIndex;
//</generated>

        public int CompareTo(ServerEntitySnapshot other)
        {
            return Entity.CompareTo(other.Entity);
        }

        public Entity SnapshotEntity => Entity;
    }
}