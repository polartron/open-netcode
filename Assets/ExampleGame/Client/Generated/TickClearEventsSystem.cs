using OpenNetcode.Shared.Systems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

//<using>
//<generated>
using ExampleGame.Shared.Movement.Components;
using ExampleGame.Shared.Components;
//</generated>

namespace Client.Generated
{
    [UpdateInGroup(typeof(TickPostSimulationSystemGroup))]
    [DisableAutoCreation]
    public partial class TickClearEventsSystem : SystemBase
    {
        //<template:publicevent>
        //private EntityQuery _##TYPELOWER##Query;
        //</template>
//<generated>
        private EntityQuery _bumpEventQuery;
//</generated>
        protected override void OnCreate()
        {
            //<template:publicevent>
            //_##TYPELOWER##Query = GetEntityQuery(
            //    ComponentType.ReadWrite<##TYPE##>());
            //</template>
//<generated>
            _bumpEventQuery = GetEntityQuery(
                ComponentType.ReadWrite<BumpEvent>());
//</generated>
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            //<template:publicevent>
            //ClearEventsJob<##TYPE##> ##TYPELOWER##Job = new ClearEventsJob<##TYPE##>()
            //{
            //    BufferTypeHandle = GetBufferTypeHandle<##TYPE##>()
            //};
            //Dependency = ##TYPELOWER##Job.ScheduleParallel(_##TYPELOWER##Query, Dependency);
            //</template>
//<generated>
            ClearEventsJob<BumpEvent> bumpEventJob = new ClearEventsJob<BumpEvent>()
            {
                BufferTypeHandle = GetBufferTypeHandle<BumpEvent>()
            };
            Dependency = bumpEventJob.ScheduleParallel(_bumpEventQuery, Dependency);
//</generated>
            
            Dependency.Complete();
        }
        
        [BurstCompile]
        public struct ClearEventsJob<T> : IJobChunk where T : unmanaged, IBufferElementData
        {
            public BufferTypeHandle<T> BufferTypeHandle;
        
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var buffers = chunk.GetBufferAccessor(BufferTypeHandle);

                for (int i = 0; i < chunk.Count; i++)
                {
                    var buffer = buffers[i];
                    buffer.Clear();
                }
            }
        }
    }
}
