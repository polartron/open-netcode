using System;
using Unity.Entities;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
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

        //<template>
        //public int ##TYPE##Index;
        //</template>
//<generated>
        public int EntityPositionIndex;
        public int EntityVelocityIndex;
//</generated>

        public int CompareTo(ServerEntitySnapshot other)
        {
            return Entity.CompareTo(other.Entity);
        }

        public Entity SnapshotEntity => Entity;
    }
}