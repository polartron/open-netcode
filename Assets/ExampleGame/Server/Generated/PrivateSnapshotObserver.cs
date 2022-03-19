using Unity.Entities;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    [InternalBufferCapacity(8)]
    public struct PrivateSnapshotObserver : IBufferElementData
    {
        public Entity Entity;
        public int ComponentInterestMask;

        public static int Observe<T>(int mask) where T : unmanaged
        {
            //<template:publicsnapshot>
            //if (typeof(T) == typeof(##TYPE##))
            //{
            //    return mask | (1 << ##INDEX##);
            //}
            //</template>
//<generated>
            if (typeof(T) == typeof(EntityVelocity))
            {
                return mask | (1 << 0);
            }
            if (typeof(T) == typeof(EntityPosition))
            {
                return mask | (1 << 1);
            }
            if (typeof(T) == typeof(PathComponent))
            {
                return mask | (1 << 2);
            }
//</generated>
            //<template:privatesnapshot>
            //if (typeof(T) == typeof(##TYPE##))
            //{
            //    return mask | (1 << ##INDEX##);
            //}
            //</template>
//<generated>
            if (typeof(T) == typeof(EntityHealth))
            {
                return mask | (1 << 3);
            }
//</generated>

            return mask;
        }
    }
}