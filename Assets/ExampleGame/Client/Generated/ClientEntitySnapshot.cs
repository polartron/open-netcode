using System;
using Unity.Entities;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
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
//</generated>

        public int CompareTo(ClientEntitySnapshot other)
        {
            return ServerId.CompareTo(other.ServerId);
        }
    }

}
