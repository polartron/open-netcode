using System;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Time;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    internal struct Area : IDisposable
    {
        private int _capacity;

        public int Capacity
        {
            get { return _capacity; }
        }

        public BaseLines<ServerEntitySnapshot> EntitySnapshotBaseLine;

        //<template>
        //public BaseLines<##TYPE##> ##TYPE##BaseLine;
        //</template>
//<generated>
        public BaseLines<EntityPosition> EntityPositionBaseLine;
        public BaseLines<EntityVelocity> EntityVelocityBaseLine;
        public BaseLines<PathComponent> PathComponentBaseLine;
//</generated>

        public Area(int capacity)
        {
            _capacity = capacity;
            EntitySnapshotBaseLine = new BaseLines<ServerEntitySnapshot>(capacity, TimeConfig.BaseSnapshotEvery);
            //<template>
            //##TYPE##BaseLine = new BaseLines<##TYPE##>(capacity, TimeConfig.BaseSnapshotEvery);
            //</template>
//<generated>
            EntityPositionBaseLine = new BaseLines<EntityPosition>(capacity, TimeConfig.BaseSnapshotEvery);
            EntityVelocityBaseLine = new BaseLines<EntityVelocity>(capacity, TimeConfig.BaseSnapshotEvery);
            PathComponentBaseLine = new BaseLines<PathComponent>(capacity, TimeConfig.BaseSnapshotEvery);
//</generated>
        }

        public void Dispose()
        {
            EntitySnapshotBaseLine.Dispose();
            //<template>
            //##TYPE##BaseLine.Dispose();
            //</template>
//<generated>
            EntityPositionBaseLine.Dispose();
            EntityVelocityBaseLine.Dispose();
            PathComponentBaseLine.Dispose();
//</generated>
        }
    }
}