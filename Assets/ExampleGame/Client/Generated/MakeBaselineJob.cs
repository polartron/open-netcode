using OpenNetcode.Client.Components;
using OpenNetcode.Shared.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    [BurstCompile]
    struct MakeBaseLineJob : IJob
    {
        [ReadOnly] public int SnapshotIndex;
        [ReadOnly] public int Tick;
        [ReadOnly] public ClientArea Area;

        [ReadOnly] public NativeHashMap<int, ClientEntitySnapshot> Entities;

        //<template:publicsnapshot>
        //[ReadOnly] public BufferFromEntity<SnapshotBufferElement<##TYPE##>> ##TYPE##Buffer;
        //</template>
//<generated>
        [ReadOnly] public BufferFromEntity<SnapshotBufferElement<EntityVelocity>> EntityVelocityBuffer;
        [ReadOnly] public BufferFromEntity<SnapshotBufferElement<EntityPosition>> EntityPositionBuffer;
        [ReadOnly] public BufferFromEntity<SnapshotBufferElement<PathComponent>> PathComponentBuffer;
//</generated>
        [ReadOnly] public ComponentDataFromEntity<NetworkedPrefabIndex> NetworkedPrefabFromEntity;

        public void Execute()
        {
            NativeArray<ClientEntitySnapshot> entitySnapshots = Entities.GetValueArray(Allocator.Temp);
            entitySnapshots.Sort();
            MakeBaselines(ref Area, entitySnapshots);
        }

        private void MakeBaselines(ref ClientArea area, in NativeArray<ClientEntitySnapshot> entitySnapshots)
        {
            NativeArray<ClientEntitySnapshot> entities =
                new NativeArray<ClientEntitySnapshot>(entitySnapshots.Length, Allocator.Temp);

            //<template:publicsnapshot>
            //NativeArray<##TYPE##> ##TYPELOWER##s = new NativeArray<##TYPE##>(entitySnapshots.Length, Allocator.Temp);
            //int ##TYPELOWER##Index = 0;
            //</template>
//<generated>
            NativeArray<EntityVelocity> entityVelocitys = new NativeArray<EntityVelocity>(entitySnapshots.Length, Allocator.Temp);
            int entityVelocityIndex = 0;
            NativeArray<EntityPosition> entityPositions = new NativeArray<EntityPosition>(entitySnapshots.Length, Allocator.Temp);
            int entityPositionIndex = 0;
            NativeArray<PathComponent> pathComponents = new NativeArray<PathComponent>(entitySnapshots.Length, Allocator.Temp);
            int pathComponentIndex = 0;
//</generated>

            for (int i = 0; i < entitySnapshots.Length; i++)
            {
                var snapshot = entitySnapshots[i];
                int mask = snapshot.ComponentMask;

                if (NetworkedPrefabFromEntity.HasComponent(snapshot.Entity))
                {
                    snapshot.Type = NetworkedPrefabFromEntity[snapshot.Entity].Value;
                }

                //<template:publicsnapshot>
                //if ((mask & (1 << ##INDEX##)) != 0)
                //{
                //    var buffer = ##TYPE##Buffer[snapshot.Entity];
                //    ##TYPELOWER##s[##TYPELOWER##Index] = buffer[SnapshotIndex % buffer.Length].Value;
                //    snapshot.##TYPE##Index = ##TYPELOWER##Index;
                //    ##TYPELOWER##Index++;
                //}
                //</template>
//<generated>
                if ((mask & (1 << 0)) != 0)
                {
                    var buffer = EntityVelocityBuffer[snapshot.Entity];
                    entityVelocitys[entityVelocityIndex] = buffer[SnapshotIndex % buffer.Length].Value;
                    snapshot.EntityVelocityIndex = entityVelocityIndex;
                    entityVelocityIndex++;
                }
                if ((mask & (1 << 1)) != 0)
                {
                    var buffer = EntityPositionBuffer[snapshot.Entity];
                    entityPositions[entityPositionIndex] = buffer[SnapshotIndex % buffer.Length].Value;
                    snapshot.EntityPositionIndex = entityPositionIndex;
                    entityPositionIndex++;
                }
                if ((mask & (1 << 2)) != 0)
                {
                    var buffer = PathComponentBuffer[snapshot.Entity];
                    pathComponents[pathComponentIndex] = buffer[SnapshotIndex % buffer.Length].Value;
                    snapshot.PathComponentIndex = pathComponentIndex;
                    pathComponentIndex++;
                }
//</generated>
                entities[i] = snapshot;
            }

            area.ClientEntitySnapshotBaseLine.UpdateBaseline(entities, SnapshotIndex, entitySnapshots.Length);

            //<template:publicsnapshot>
            //area.##TYPE##BaseLine.UpdateBaseline(##TYPELOWER##s, SnapshotIndex, ##TYPELOWER##Index);
            //</template>
//<generated>
            area.EntityVelocityBaseLine.UpdateBaseline(entityVelocitys, SnapshotIndex, entityVelocityIndex);
            area.EntityPositionBaseLine.UpdateBaseline(entityPositions, SnapshotIndex, entityPositionIndex);
            area.PathComponentBaseLine.UpdateBaseline(pathComponents, SnapshotIndex, pathComponentIndex);
//</generated>
        }
    }
}