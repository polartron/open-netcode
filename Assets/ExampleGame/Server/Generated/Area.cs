using System;
using OpenNetcode.Movement.Components;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Time;
using Server.Generated;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
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
//</generated>
        }
    }
}