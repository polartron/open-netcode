
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

//<using>
//<generated>
using OpenNetcode.Movement.Components;
using Shared.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Server.Generated
{
    [BurstCompile]
    public struct ClearEventsJob : IJobEntityBatch
    {
        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
        //<events>
        //public BufferFromEntity<##TYPE##> ##TYPE##BufferFromEntity;
        //</events>
//<generated>
        public BufferFromEntity<BumpEvent> BumpEventBufferFromEntity;
//</generated>
        
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var entities = batchInChunk.GetNativeArray(EntityTypeHandle);

            for (int i = 0; i < batchInChunk.Count; i++)
            {
                var entity = entities[i];

                //<events>
                //if (##TYPE##BufferFromEntity.HasComponent(entity))
                //{
                //    ##TYPE##BufferFromEntity[entity].Clear();
                //}
                //</events>
//<generated>
                if (BumpEventBufferFromEntity.HasComponent(entity))
                {
                    BumpEventBufferFromEntity[entity].Clear();
                }
//</generated>
            }
        }
    }
}
