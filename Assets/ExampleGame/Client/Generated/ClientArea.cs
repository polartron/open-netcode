using System;
using OpenNetcode.Shared;
using OpenNetcode.Shared.Time;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
using ExampleGame.Shared.Components;
//</generated>


namespace Client.Generated
{
    public struct ClientArea : IDisposable
    {
        public BaseLines<ClientEntitySnapshot> ClientEntitySnapshotBaseLine;
        //<template>
        //public BaseLines<##TYPE##> ##TYPE##BaseLine;
        //</template>
//<generated>
        public BaseLines<EntityPosition> EntityPositionBaseLine;
        public BaseLines<EntityVelocity> EntityVelocityBaseLine;
//</generated>
        
        public ClientArea(int capacity)
        {
            ClientEntitySnapshotBaseLine = new BaseLines<ClientEntitySnapshot>(capacity, TimeConfig.BaseSnapshotEvery);
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
            ClientEntitySnapshotBaseLine.Dispose();
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