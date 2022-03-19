using System;
using System.Collections.Generic;
using AOT;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Networking.Transport.Protocols;
using Unity.Networking.Transport.Utilities;

namespace Unity.Networking.Transport.Samples
{
#if NET_DOTS
    public class NetworkInterfaceException : Exception
    {
        public NetworkInterfaceException(int err)
            : base("Socket error: " + err)
        {
        }
    }
#else
    public class NetworkInterfaceException : System.Net.Sockets.SocketException
    {
        public NetworkInterfaceException(int err)
            : base(err)
        {
        }
    }
#endif

    public struct UDPNetworkInterface : INetworkInterface
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private class SocketList
        {
            public HashSet<long> OpenSockets = new HashSet<long>();

            ~SocketList()
            {
                foreach (var socket in OpenSockets)
                {
                    long sockHand = socket;
                    int errorcode = 0;
                    NativeBindings.network_close(ref sockHand, ref errorcode);
                }
            }
        }
        private static SocketList AllSockets = new SocketList();
#endif

        NativeArray<long> m_UserData;

        public unsafe NetworkInterfaceEndPoint LocalEndPoint
        {
            get
            {
                var localEndPoint = new network_address {length = network_address.Length};
                int errorcode = 0;
                var result = NativeBindings.network_get_socket_address(m_UserData[0], ref localEndPoint, ref errorcode);
                if (result != 0)
                {
                    throw new NetworkInterfaceException(errorcode);
                }

                var endpoint = default(NetworkInterfaceEndPoint);
                endpoint.dataLength = UnsafeUtility.SizeOf<network_address>();
                UnsafeUtility.MemCpy(endpoint.data, &localEndPoint, endpoint.dataLength);
                return endpoint;
            }
        }

        private static unsafe NetworkInterfaceEndPoint ParseNetworkAddress(NetworkEndPoint endPoint)
        {
            NetworkInterfaceEndPoint ep = default(NetworkInterfaceEndPoint);
            var addr = (network_address*)ep.data;
            var sai = (sockaddr_in*)addr->data;
#if (UNITY_EDITOR_OSX || ((UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR))
            sai->sin_family.sa_family = (byte)NetworkFamily.Ipv4;
            sai->sin_family.sa_len = (byte)sizeof(sockaddr_in);
#else
            sai->sin_family.sa_family = (ushort)NetworkFamily.Ipv4;
#endif
            sai->sin_port = endPoint.RawPort;
            var bytes = endPoint.GetRawAddressBytes();
            sai->sin_addr.s_addr = *(uint*)bytes.GetUnsafeReadOnlyPtr();

            addr->length = sizeof(sockaddr_in);
            ep.dataLength = sizeof(network_address);
            return ep;
        }

        public int CreateInterfaceEndPoint(NetworkEndPoint address, out NetworkInterfaceEndPoint endpoint)
        {
            if (address.Family != NetworkFamily.Ipv4)
                throw new ArgumentException("Invalid family type");

            endpoint = ParseNetworkAddress(address);
            return 0;
        }

        public unsafe NetworkEndPoint GetGenericEndPoint(NetworkInterfaceEndPoint endpoint)
        {
            var address = NetworkEndPoint.AnyIpv4;
            var addr = (network_address*)endpoint.data;
            var sai = (sockaddr_in*)addr->data;
            address.RawPort = sai->sin_port;
            if (sai->sin_addr.s_addr != 0)
            {
                var bytes = new NativeArray<byte>(4, Allocator.Temp);
                UnsafeUtility.MemCpy(bytes.GetUnsafePtr(), UnsafeUtility.AddressOf(ref sai->sin_addr.s_addr),  4);
                address.SetRawAddressBytes(bytes);
            }
            return address;
        }

        public unsafe int Initialize(NetworkSettings settings)
        {
            NativeBindings.network_initialize();

            var ep = default(NetworkInterfaceEndPoint);
            var result = 0;

            if ((result = CreateInterfaceEndPoint(NetworkEndPoint.AnyIpv4, out ep)) != (int)Error.StatusCode.Success)
                return result;

            long sockHand;
            int ret = CreateAndBindSocket(out sockHand, *(network_address*)ep.data);

            if (ret == 0)
            {
                m_UserData = new NativeArray<long>(1, Allocator.Persistent);
                m_UserData[0] = sockHand;
            }
            return ret;
        }

        public void Dispose()
        {
            Close();
            NativeBindings.network_terminate();
            m_UserData.Dispose();
        }

        struct ReceiveJob : IJob
        {
            public NetworkPacketReceiver receiver;
            public long socket;

            public unsafe void Execute()
            {
                var address = new network_address {length = network_address.Length};
                receiver.ReceiveErrorCode = 0;

                while (true)
                {
                    var size = NetworkParameterConstants.MTU;
                    var ptr = receiver.AllocateMemory(ref size);
                    if (ptr == IntPtr.Zero)
                        return;

                    var resultReceive = NativeReceive((byte*)ptr.ToPointer(), size, ref address);
                    if (resultReceive <= 0)
                    {
                        if (resultReceive != 0)
                            receiver.ReceiveErrorCode = -resultReceive;
                        return;
                    }

                    var endpoint = default(NetworkInterfaceEndPoint);
                    endpoint.dataLength = UnsafeUtility.SizeOf<network_address>();
                    UnsafeUtility.MemCpy(endpoint.data, &address, endpoint.dataLength);

                    var resultAppend = receiver.AppendPacket(ptr, ref endpoint, resultReceive, NetworkPacketReceiver.AppendPacketMode.NoCopyNeeded);
                    if (resultAppend == false)
                        return;
                }
            }

            unsafe int NativeReceive(void* data, int length, ref network_address address)
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                if (length <= 0)
                    throw new ArgumentException("Can't receive into 0 bytes or less of buffer memory");
#endif
                var iov = new network_iovec
                {
                    buf = data,
                    len = length,
                };

                int errorcode = 0;
                var result = NativeBindings.network_recvmsg(socket, &iov, 1, ref address, ref errorcode);
                if (result == -1)
                {
                    if (errorcode == 10035 || errorcode == 35 || errorcode == 11)
                        return 0;

                    receiver.ReceiveErrorCode = errorcode;
                }
                return result;
            }
        }

        public JobHandle ScheduleReceive(NetworkPacketReceiver receiver, JobHandle dep)
        {
            var job = new ReceiveJob {receiver = receiver, socket = m_UserData[0]};
            return job.Schedule(dep);
        }

        public JobHandle ScheduleSend(NativeQueue<QueuedSendMessage> sendQueue, JobHandle dep)
        {
            return dep;
        }

        public unsafe int Bind(NetworkInterfaceEndPoint endpoint)
        {
            long newSocket;
            int ret = CreateAndBindSocket(out newSocket, *(network_address*)endpoint.data);
            if (ret != 0)
                return ret;
            Close();

            m_UserData[0] = newSocket;

            return 0;
        }

        public int Listen()
        {
            return 0;
        }

        private void Close()
        {
            if (m_UserData[0] < 0)
                return;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AllSockets.OpenSockets.Remove(m_UserData[0]);
#endif
            int errorcode = 0;
            long sockHand = m_UserData[0];
            NativeBindings.network_close(ref sockHand, ref errorcode);

            m_UserData[0] = -1;
        }

        static TransportFunctionPointer<NetworkSendInterface.BeginSendMessageDelegate> BeginSendMessageFunctionPointer = new TransportFunctionPointer<NetworkSendInterface.BeginSendMessageDelegate>(BeginSendMessage);
        static TransportFunctionPointer<NetworkSendInterface.EndSendMessageDelegate> EndSendMessageFunctionPointer = new TransportFunctionPointer<NetworkSendInterface.EndSendMessageDelegate>(EndSendMessage);
        static TransportFunctionPointer<NetworkSendInterface.AbortSendMessageDelegate> AbortSendMessageFunctionPointer = new TransportFunctionPointer<NetworkSendInterface.AbortSendMessageDelegate>(AbortSendMessage);
        public unsafe NetworkSendInterface CreateSendInterface()
        {
            return new NetworkSendInterface
            {
                BeginSendMessage = BeginSendMessageFunctionPointer,
                EndSendMessage = EndSendMessageFunctionPointer,
                AbortSendMessage = AbortSendMessageFunctionPointer,
                UserData = (IntPtr)m_UserData.GetUnsafePtr()
            };
        }

        [MonoPInvokeCallback(typeof(NetworkSendInterface.BeginSendMessageDelegate))]
        public static unsafe int BeginSendMessage(out NetworkInterfaceSendHandle handle, IntPtr userData, int requiredPayloadSize)
        {
            handle.id = 0;
            handle.size = 0;
            handle.capacity = requiredPayloadSize;
            handle.data = (IntPtr)UnsafeUtility.Malloc(handle.capacity, 8, Allocator.Temp);
            handle.flags = default;
            return 0;
        }

        [MonoPInvokeCallback(typeof(NetworkSendInterface.EndSendMessageDelegate))]
        public static unsafe int EndSendMessage(ref NetworkInterfaceSendHandle handle, ref NetworkInterfaceEndPoint address, IntPtr userData, ref NetworkSendQueueHandle sendQueue)
        {
            network_iovec iov;
            iov.buf = (void*)handle.data;
            iov.len = handle.size;
            int errorcode = 0;
            var addr = address;
            return NativeBindings.network_sendmsg(*(long*)userData, &iov, 1, ref *(network_address*)addr.data, ref errorcode);
        }

        [MonoPInvokeCallback(typeof(NetworkSendInterface.AbortSendMessageDelegate))]
        private static void AbortSendMessage(ref NetworkInterfaceSendHandle handle, IntPtr userData)
        {
        }

        int CreateAndBindSocket(out long socket, network_address address)
        {
            socket = -1;
            int errorcode = 0;
            int ret = NativeBindings.network_create_and_bind(ref socket, ref address, ref errorcode);
            if (ret != 0)
                return errorcode;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AllSockets.OpenSockets.Add(socket);
#endif
            if ((ret = NativeBindings.network_set_nonblocking(socket, ref errorcode)) != 0)
                return errorcode;
            if ((ret = NativeBindings.network_set_send_buffer_size(socket, ushort.MaxValue, ref errorcode)) != 0)
                return errorcode;
            if ((ret = NativeBindings.network_set_receive_buffer_size(socket, ushort.MaxValue, ref errorcode)) != 0)
                return errorcode;
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
            // Avoid WSAECONNRESET errors when sending to an endpoint which isn't open yet (unclean connect/disconnects)
            NativeBindings.network_set_connection_reset(socket, 0);
#endif
            return 0;
        }
    }
}
