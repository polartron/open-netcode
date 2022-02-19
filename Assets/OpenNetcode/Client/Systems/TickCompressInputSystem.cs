using OpenNetcode.Shared.Components;
using OpenNetcode.Shared.Systems;
using Unity.Entities;
using Unity.Networking.Transport;
using Allocator = Unity.Collections.Allocator;

namespace OpenNetcode.Client.Systems
{
    /// <summary>
    /// The client simulation receives uncompressed input from the player.
    /// We need to compress it as if it has been received from the server so that we don't get mispredictions.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    [UpdateInGroup(typeof(TickPreSimulationSystemGroup), OrderFirst = true)]
    public class TickCompressInputSystem<TInput> : SystemBase
        where TInput : unmanaged, INetworkedComponent
    {
        private NetworkCompressionModel _compressionModel;
        protected override void OnCreate()
        {
            _compressionModel = new NetworkCompressionModel(Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var clientData = GetSingleton<ClientData>();
            Entity clientEntity = clientData.LocalPlayer;

            if (clientEntity == Entity.Null)
                return;

            DataStreamWriter writer = new DataStreamWriter(10, Allocator.Temp);
            TInput input = EntityManager.GetComponentData<TInput>(clientEntity);
            input.Write(ref writer, _compressionModel);
            DataStreamReader reader = new DataStreamReader(writer.AsNativeArray());
            input.Read(ref reader, _compressionModel);
            EntityManager.SetComponentData(clientEntity, input);
        }
    }
}