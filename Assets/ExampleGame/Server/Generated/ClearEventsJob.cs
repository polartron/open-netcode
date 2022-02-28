
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace ExampleGame.Server.Generated
{
    [BurstCompile]
    public struct ClearEventsJob : IJobEntityBatch
    {
        [ReadOnly] public EntityTypeHandle EntityTypeHandle;
        //<template:publicevent>
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

                //<template:publicevent>
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
