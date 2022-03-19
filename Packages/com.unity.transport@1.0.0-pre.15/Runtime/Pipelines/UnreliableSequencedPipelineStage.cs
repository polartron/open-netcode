using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Networking.Transport.Utilities;

namespace Unity.Networking.Transport
{
    /// <summary>
    /// The UnreliableSequencedPipelineStage is used to send unreliable packets in order.
    /// </summary>
    [BurstCompile]
    public unsafe struct UnreliableSequencedPipelineStage : INetworkPipelineStage
    {
        static TransportFunctionPointer<NetworkPipelineStage.ReceiveDelegate> ReceiveFunctionPointer = new TransportFunctionPointer<NetworkPipelineStage.ReceiveDelegate>(Receive);
        static TransportFunctionPointer<NetworkPipelineStage.SendDelegate> SendFunctionPointer = new TransportFunctionPointer<NetworkPipelineStage.SendDelegate>(Send);
        static TransportFunctionPointer<NetworkPipelineStage.InitializeConnectionDelegate> InitializeConnectionFunctionPointer = new TransportFunctionPointer<NetworkPipelineStage.InitializeConnectionDelegate>(InitializeConnection);
        /// <summary>
        /// Statics the initialize using the specified static instance buffer
        /// </summary>
        /// <param name="staticInstanceBuffer">The static instance buffer</param>
        /// <param name="staticInstanceBufferLength">The static instance buffer length</param>
        /// <param name="settings">The net params</param>
        /// <returns>The network pipeline stage</returns>
        public NetworkPipelineStage StaticInitialize(byte* staticInstanceBuffer, int staticInstanceBufferLength, NetworkSettings settings)
        {
            return new NetworkPipelineStage(
                Receive: ReceiveFunctionPointer,
                Send: SendFunctionPointer,
                InitializeConnection: InitializeConnectionFunctionPointer,
                ReceiveCapacity: UnsafeUtility.SizeOf<int>(),
                SendCapacity: UnsafeUtility.SizeOf<int>(),
                HeaderCapacity: UnsafeUtility.SizeOf<ushort>(),
                SharedStateCapacity: 0
            );
        }

        /// <summary>
        /// Gets the value of the static size
        /// </summary>
        public int StaticSize => 0;

        [BurstCompile(DisableDirectCall = true)]
        [MonoPInvokeCallback(typeof(NetworkPipelineStage.ReceiveDelegate))]
        private static void Receive(ref NetworkPipelineContext ctx, ref InboundRecvBuffer inboundBuffer, ref NetworkPipelineStage.Requests requests, int systemHeaderSize)
        {
            var inboundArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(inboundBuffer.buffer, inboundBuffer.bufferLength, Allocator.Invalid);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            var safetyHandle = AtomicSafetyHandle.GetTempMemoryHandle();
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref inboundArray, safetyHandle);
#endif
            var reader = new DataStreamReader(inboundArray);
            var oldSequenceId = (int*)ctx.internalProcessBuffer;
            ushort sequenceId = reader.ReadUShort();

            if (SequenceHelpers.GreaterThan16(sequenceId, (ushort)*oldSequenceId))
            {
                *oldSequenceId = sequenceId;
                // Skip over the part of the buffer which contains the header
                inboundBuffer = inboundBuffer.Slice(sizeof(ushort));
                return;
            }
            inboundBuffer = default;
        }

        [BurstCompile(DisableDirectCall = true)]
        [MonoPInvokeCallback(typeof(NetworkPipelineStage.SendDelegate))]
        private static int Send(ref NetworkPipelineContext ctx, ref InboundSendBuffer inboundBuffer, ref NetworkPipelineStage.Requests requests, int systemHeaderSize)
        {
            var sequenceId = (int*)ctx.internalProcessBuffer;
            ctx.header.WriteUShort((ushort)*sequenceId);
            *sequenceId = (ushort)(*sequenceId + 1);
            return (int)Error.StatusCode.Success;
        }

        [BurstCompile(DisableDirectCall = true)]
        [MonoPInvokeCallback(typeof(NetworkPipelineStage.InitializeConnectionDelegate))]
        private static void InitializeConnection(byte* staticInstanceBuffer, int staticInstanceBufferLength,
            byte* sendProcessBuffer, int sendProcessBufferLength, byte* recvProcessBuffer, int recvProcessBufferLength,
            byte* sharedProcessBuffer, int sharedProcessBufferLength)
        {
            if (recvProcessBufferLength > 0)
            {
                // The receive processing buffer contains the current sequence ID, initialize it to -1 as it will be incremented when used.
                *(int*)recvProcessBuffer = -1;
            }
        }
    }
}
