using System;
using Server.Generated;
using Unity.Collections;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    internal struct SnapshotStaging : IDisposable
    {
        public struct UpdatedData
        {
            public bool Added;
            public ServerEntitySnapshot Base;
        }

        public struct RemovedOrAddedData
        {
            public int Index;
            public bool Added;
            public int Type;
        }

        public NativeList<RemovedOrAddedData> RemovedOrAdded;
        public NativeList<UpdatedData> Updated;

        public SnapshotStaging(int capacity)
        {
            RemovedOrAdded = new NativeList<RemovedOrAddedData>(capacity, Allocator.Temp);
            Updated = new NativeList<UpdatedData>(capacity, Allocator.Temp);
        }

        public void Dispose()
        {
            RemovedOrAdded.Dispose();
            Updated.Dispose();
        }
        
        public static SnapshotStaging Create(in NativeSlice<ServerEntitySnapshot> baseLine, in NativeArray<ServerEntitySnapshot> current, int capacity = 100)
        {
            SnapshotStaging staging = new SnapshotStaging(capacity);

            int seek = 0;

            for (int i = 0; i < current.Length; i++)
            {
                var ce = current[i];
                var c = ce.SnapshotEntity.Index;
                for (; seek < baseLine.Length;)
                {
                    ServerEntitySnapshot baseSnapshot = baseLine[seek];
                    var b = baseSnapshot.Entity.Index;

                    if (b < c)
                    {
                        // b has been removed
                        staging.RemovedOrAdded.Add(new RemovedOrAddedData()
                        {
                            Index = b,
                            Added = false,
                        });
                        seek++;
                    }
                    else if (b == c) // changed
                    {
                        staging.Updated.Add(new UpdatedData()
                        {
                            Base = baseSnapshot,
                            Added = false
                        });
                        seek++;
                        break;
                    }
                    else if (b > c)
                    {
                        staging.RemovedOrAdded.Add(new RemovedOrAddedData()
                        {
                            Index = c,
                            Added = true,
                            Type = ce.PrefabType
                        });

                        staging.Updated.Add(new UpdatedData()
                        {
                            Base = default,
                            Added = true
                        });
                        break;
                    }
                }
                
                //if (seek == baseLine.Length && seek < current.Length)
                if (seek == baseLine.Length)
                {
                    //Remainder of current have been added
                    for (int j = i; j < current.Length; j++)
                    {
                        staging.RemovedOrAdded.Add(new SnapshotStaging.RemovedOrAddedData()
                        {
                            Index = current[j].Entity.Index,
                            Added = true,
                            Type = current[j].PrefabType
                        });

                        staging.Updated.Add(new SnapshotStaging.UpdatedData()
                        {
                            Base = default,
                            Added = true
                        });
                    }

                    break;
                }
            }

            if (seek < baseLine.Length)
            {
                //Remainder of baseline have been removed
                for (int j = seek; j < baseLine.Length; j++)
                {
                    staging.RemovedOrAdded.Add(new SnapshotStaging.RemovedOrAddedData()
                    {
                        Index = baseLine[j].SnapshotEntity.Index,
                        Added = false
                    });
                }
            }

            return staging;
        }
    }
}