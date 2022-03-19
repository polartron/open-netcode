using OpenNetcode.Shared.Systems;
using Unity.Collections;
using Unity.Entities;

namespace OpenNetcode.Client.Systems
{
    public abstract partial class TickEventCallbackSystem<T> : SystemBase where T : unmanaged, IBufferElementData
    {
        private EntityQuery _query;

        protected abstract void OnEvent(Entity entity, T invokedEvent);

        protected override void OnCreate()
        {
            _query = GetEntityQuery(ComponentType.ReadOnly<T>());
        }

        protected override void OnUpdate()
        {
            var buffers = GetBufferFromEntity<T>();
            var entities = _query.ToEntityArray(Allocator.Temp);

            for (var i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var buffer = buffers[entity];

                for (var j = 0; j < buffer.Length; j++)
                {
                    var bufferElement = buffer[j];
                    OnEvent(entity, bufferElement);
                }
            }
        }
    }
}