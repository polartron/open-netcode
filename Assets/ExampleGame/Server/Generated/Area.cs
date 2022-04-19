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

        //<template:publicsnapshot>
        //public BaseLines<##TYPE##> ##TYPE##BaseLine;
        //</template>
//<generated>
        public BaseLines<EntityVelocity> EntityVelocityBaseLine;
        public BaseLines<PathComponent> PathComponentBaseLine;
        public BaseLines<EntityPosition> EntityPositionBaseLine;
//</generated>

        public Area(int capacity)
        {
            _capacity = capacity;
            EntitySnapshotBaseLine = new BaseLines<ServerEntitySnapshot>(capacity, TimeConfig.SnapshotsPerSecond);
            //<template:publicsnapshot>
            //##TYPE##BaseLine = new BaseLines<##TYPE##>(capacity, TimeConfig.SnapshotsPerSecond);
            //</template>
//<generated>
            EntityVelocityBaseLine = new BaseLines<EntityVelocity>(capacity, TimeConfig.SnapshotsPerSecond);
            PathComponentBaseLine = new BaseLines<PathComponent>(capacity, TimeConfig.SnapshotsPerSecond);
            EntityPositionBaseLine = new BaseLines<EntityPosition>(capacity, TimeConfig.SnapshotsPerSecond);
//</generated>
        }

        public void Dispose()
        {
            EntitySnapshotBaseLine.Dispose();
            //<template:publicsnapshot>
            //##TYPE##BaseLine.Dispose();
            //</template>
//<generated>
            EntityVelocityBaseLine.Dispose();
            PathComponentBaseLine.Dispose();
            EntityPositionBaseLine.Dispose();
//</generated>
        }
    }
}