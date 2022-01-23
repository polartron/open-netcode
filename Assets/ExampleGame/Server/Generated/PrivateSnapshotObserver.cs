using OpenNetcode.Movement.Components;
using OpenNetcode.Shared.Components;
using Shared.Components;
using Unity.Entities;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    [InternalBufferCapacity(8)]
    public struct PrivateSnapshotObserver : IBufferElementData
    {
        public Entity Entity;
        public int ComponentInterestMask;

        public static int Observe<T>(int mask) where T : unmanaged, ISnapshotComponent<T>
        {
            //<template>
            //if (typeof(T) == typeof(##TYPE##))
            //{
            //    return mask | (1 << ##INDEX##);
            //}
            //</template>
//<generated>
            if (typeof(T) == typeof(EntityPosition))
            {
                return mask | (1 << 0);
            }
            if (typeof(T) == typeof(EntityVelocity))
            {
                return mask | (1 << 1);
            }
//</generated>
            //<privatetemplate>
            //if (typeof(T) == typeof(##TYPE##))
            //{
            //    return mask | (1 << ##INDEX##);
            //}
            //</privatetemplate>
//<generated>
            if (typeof(T) == typeof(EntityHealth))
            {
                return mask | (1 << 2);
            }
//</generated>

            return mask;
        }
    }
}