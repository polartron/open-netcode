using System;
using Unity.Entities;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    public struct ClientEntitySnapshot : IComparable<ClientEntitySnapshot>
    {
        public int ServerId;
        public Entity Entity;
        public int Type;
        public int ComponentMask;

        //<template>
        //public int ##TYPE##Index;
        //</template>
//<generated>
        public int EntityPositionIndex;
        public int EntityVelocityIndex;
        public int PathComponentIndex;
//</generated>

        public int CompareTo(ClientEntitySnapshot other)
        {
            return ServerId.CompareTo(other.ServerId);
        }
    }

}
