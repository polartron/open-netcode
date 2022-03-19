using System;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Time;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>


namespace Client.Generated
{
    public struct ClientArea : IDisposable
    {
        public BaseLines<ClientEntitySnapshot> ClientEntitySnapshotBaseLine;
        //<template:publicsnapshot>
        //public BaseLines<##TYPE##> ##TYPE##BaseLine;
        //</template>
//<generated>
        public BaseLines<EntityVelocity> EntityVelocityBaseLine;
        public BaseLines<EntityPosition> EntityPositionBaseLine;
        public BaseLines<PathComponent> PathComponentBaseLine;
//</generated>
        
        public ClientArea(int capacity)
        {
            ClientEntitySnapshotBaseLine = new BaseLines<ClientEntitySnapshot>(capacity, TimeConfig.SnapshotsPerSecond);
            //<template:publicsnapshot>
            //##TYPE##BaseLine = new BaseLines<##TYPE##>(capacity, TimeConfig.SnapshotsPerSecond);
            //</template>
//<generated>
            EntityVelocityBaseLine = new BaseLines<EntityVelocity>(capacity, TimeConfig.SnapshotsPerSecond);
            EntityPositionBaseLine = new BaseLines<EntityPosition>(capacity, TimeConfig.SnapshotsPerSecond);
            PathComponentBaseLine = new BaseLines<PathComponent>(capacity, TimeConfig.SnapshotsPerSecond);
//</generated>
        }

        public void Dispose()
        {
            ClientEntitySnapshotBaseLine.Dispose();
            //<template:publicsnapshot>
            //##TYPE##BaseLine.Dispose();
            //</template>
//<generated>
            EntityVelocityBaseLine.Dispose();
            EntityPositionBaseLine.Dispose();
            PathComponentBaseLine.Dispose();
//</generated>
        }
    }
}