using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Networking.Transport.Error;
using UnityEngine;
using Unity.Networking.Transport.Protocols;
using Unity.Networking.Transport.Utilities;
using UnityEngine.TestTools;
using Random = UnityEngine.Random;

namespace Unity.Networking.Transport.Tests.Utilities
{
    using System.Linq;
    public static class Random
    {
        private static System.Random random = new System.Random();

        public static string String(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

namespace Unity.Networking.Transport.Tests
{
    public struct LocalDriverHelper : IDisposable
    {
        public NetworkEndPoint EndPoint { get; }
        public NetworkDriver m_LocalDriver;
        private NativeArray<byte> m_LocalData;
        public NetworkConnection Connection { get; internal set; }
        public List<NetworkConnection> ClientConnections;

        public LocalDriverHelper(NetworkEndPoint endpoint, NetworkSettings settings = default)
        {
            m_LocalDriver = new NetworkDriver(new IPCNetworkInterface(), settings);
            m_LocalData = new NativeArray<byte>(NetworkParameterConstants.MTU, Allocator.Persistent);

            if (endpoint.IsValid)
            {
                EndPoint = endpoint;
            }
            else
            {
                EndPoint = NetworkEndPoint.LoopbackIpv4.WithPort(1);
            }

            Connection = default(NetworkConnection);
            ClientConnections = new List<NetworkConnection>();
        }

        public void Dispose()
        {
            m_LocalDriver.Dispose();
            m_LocalData.Dispose();
        }

        public void Update()
        {
            m_LocalDriver.ScheduleUpdate().Complete();
        }

        public NetworkConnection Accept()
        {
            return m_LocalDriver.Accept();
        }

        public void Host()
        {
            m_LocalDriver.Bind(EndPoint);
            m_LocalDriver.Listen();
        }

        public void Connect(NetworkEndPoint endpoint)
        {
            Assert.True(endpoint.IsValid);
            Connection = m_LocalDriver.Connect(endpoint);
            m_LocalDriver.ScheduleUpdate().Complete();
        }

        public unsafe void Assert_GotConnectionRequest(NetworkEndPoint from, bool accept = false)
        {
            int length;
            NetworkInterfaceEndPoint remote;
            Assert.True(EndPoint.IsLoopback || EndPoint.IsAny);
            Assert.True(from.IsLoopback || from.IsAny);
            var localEndPoint = IPCManager.Instance.CreateEndPoint(EndPoint.Port);
            var fromEndPoint = IPCManager.Instance.CreateEndPoint(from.Port);
            Assert.True(
                IPCManager.Instance.PeekNext(localEndPoint, m_LocalData.GetUnsafePtr(), out length, out remote) >=
                UdpCHeader.Length);

            UdpCHeader header = new UdpCHeader();
            var reader = new DataStreamReader(m_LocalData.GetSubArray(0, UdpCHeader.Length));
            Assert.True(reader.IsCreated);

            reader.ReadBytes((byte*)&header, UdpCHeader.Length);
            Assert.AreEqual((int)UdpCProtocol.ConnectionRequest, header.Type);

            Assert.True(remote == fromEndPoint);

            if (accept)
            {
                m_LocalDriver.ScheduleUpdate().Complete();
                var con = m_LocalDriver.Accept();
                ClientConnections.Add(con);
                Assert.True(con != default(NetworkConnection));
            }
        }

        public unsafe void Assert_GotDisconnectionRequest(NetworkEndPoint from)
        {
            int length;
            NetworkInterfaceEndPoint remote;
            Assert.True(EndPoint.IsLoopback || EndPoint.IsAny);
            Assert.True(from.IsLoopback || from.IsAny);
            var localEndPoint = IPCManager.Instance.CreateEndPoint(EndPoint.Port);
            var fromEndPoint = IPCManager.Instance.CreateEndPoint(from.Port);
            Assert.True(
                IPCManager.Instance.PeekNext(localEndPoint, m_LocalData.GetUnsafePtr(), out length, out remote) >=
                UdpCHeader.Length);

            UdpCHeader header = new UdpCHeader();
            var reader = new DataStreamReader(m_LocalData.GetSubArray(0, UdpCHeader.Length));
            Assert.True(reader.IsCreated);
            reader.ReadBytes((byte*)&header, UdpCHeader.Length);
            Assert.True(header.Type == (int)UdpCProtocol.Disconnect);

            Assert.True(remote == fromEndPoint);
        }

        public unsafe void Assert_GotDataRequest(NetworkEndPoint from, byte[] dataToCompare)
        {
            NetworkInterfaceEndPoint remote = default;
            int headerLen = UdpCHeader.Length;
            void* packetData = (byte*)m_LocalData.GetUnsafePtr();
            int payloadLen = NetworkParameterConstants.MTU;
            int dataLen = 0;
            Assert.True(EndPoint.IsLoopback || EndPoint.IsAny);
            Assert.True(from.IsLoopback || from.IsAny);
            var localEndPoint = IPCManager.Instance.CreateEndPoint(EndPoint.Port);
            var fromEndPoint = IPCManager.Instance.CreateEndPoint(from.Port);
            dataLen = IPCManager.Instance.ReceiveMessageEx(localEndPoint, packetData, payloadLen, ref remote);

            payloadLen = dataLen - headerLen;
            if (payloadLen <= 0)
            {
                payloadLen = 0;
            }

            UdpCHeader header = new UdpCHeader();
            var reader = new DataStreamReader(m_LocalData.GetSubArray(0, headerLen));
            Assert.True(reader.IsCreated);
            reader.ReadBytes((byte*)&header, headerLen);
            Assert.True(header.Type == (int)UdpCProtocol.Data);

            Assert.True(remote == fromEndPoint);

            Assert.True(payloadLen == dataToCompare.Length);

            reader = new DataStreamReader(m_LocalData.GetSubArray(headerLen, dataToCompare.Length));
            var received = new NativeArray<byte>(dataToCompare.Length, Allocator.Temp);
            reader.ReadBytes(received);

            for (int i = 0, n = dataToCompare.Length; i < n; ++i)
                Assert.True(received[i] == dataToCompare[i]);
        }

        public unsafe void Assert_PopEventForConnection(NetworkConnection connection, NetworkEvent.Type evnt)
        {
            DataStreamReader reader;
            var retval = m_LocalDriver.PopEventForConnection(connection, out reader);
            Assert.True(retval == evnt);
        }

        public unsafe void Assert_PopEvent(out NetworkConnection connection, NetworkEvent.Type evnt)
        {
            DataStreamReader reader;

            var retval = m_LocalDriver.PopEvent(out connection, out reader);
            Assert.True(retval == evnt);
        }
    }

    public class NetworkDriverUnitTests
    {
        private const string backend = "baselib";
        [Test]
        public void InitializeAndDestroyDriver()
        {
            var driver = new NetworkDriver(new IPCNetworkInterface());
            driver.Dispose();
        }

        [Test]
        public void BindDriverToAEndPoint()
        {
            var driver = new NetworkDriver(new IPCNetworkInterface());

            driver.Bind(NetworkEndPoint.LoopbackIpv4);
            driver.Dispose();
        }

        [Test]
        public void NoErrorOnUnboundUpdate()
        {
            using var driver = NetworkDriver.Create();
            driver.ScheduleUpdate().Complete();
        }

        [Test]
        public void ListenOnDriver()
        {
            var driver = new NetworkDriver(new IPCNetworkInterface());

            // Make sure we Bind before we Listen.
            driver.Bind(NetworkEndPoint.LoopbackIpv4);
            driver.Listen();

            Assert.True(driver.Listening);
            driver.Dispose();
        }

        [Test]
        public void AcceptNewConnectionsOnDriver()
        {
            var driver = new NetworkDriver(new IPCNetworkInterface());

            // Make sure we Bind before we Listen.
            driver.Bind(NetworkEndPoint.LoopbackIpv4);
            driver.Listen();

            Assert.True(driver.Listening);

            //NetworkConnection connection;
            while ((/*connection =*/ driver.Accept()) != default(NetworkConnection))
            {
                //Assert.True(connectionId != NetworkParameterConstants.InvalidConnectionId);
            }

            driver.Dispose();
        }

        [Test]
        public void ConnectToARemoteEndPoint()
        {
            using (var host = new LocalDriverHelper(default(NetworkEndPoint)))
            using (var driver = new NetworkDriver(new IPCNetworkInterface()))
            {
                host.Host();

                NetworkConnection connectionId = driver.Connect(host.EndPoint);
                Assert.True(connectionId != default(NetworkConnection));
                driver.ScheduleUpdate().Complete();

                var local = driver.LocalEndPoint();
                host.Assert_GotConnectionRequest(local);
            }
        }

        [Test]
        public void GetNotValidConnectionState()
        {
            using (var driver = new NetworkDriver(new IPCNetworkInterface()))
            {
                Assert.AreEqual(NetworkConnection.State.Disconnected, driver.GetConnectionState(new NetworkConnection() {m_NetworkId = Int16.MaxValue}));
                Assert.AreEqual(NetworkConnection.State.Disconnected, driver.GetConnectionState(new NetworkConnection() {m_NetworkId = -1}));
            }
        }

        // TODO: Add tests where connection attempts are exceeded (connect fails)
        // TODO: Test dropped connection accept messages (accept retries happen)
        // TODO: Needs a way to explicitly assert on connect attempt stats
        // In this test multiple connect requests are received on the server, from client, might be this is expected
        // because of how the IPC driver works, but this situation is handled properly at least by basic driver logic.
        [Test]
        public void ConnectAttemptWithRetriesToARemoteEndPoint()
        {
            NetworkConnection connection;
            NetworkEvent.Type eventType = 0;
            DataStreamReader reader;

            var settings = new NetworkSettings();
            settings.WithNetworkConfigParameters(connectTimeoutMS: 15, maxConnectAttempts: 10, fixedFrameTimeMS: 10);

            // Tiny connect timeout for this test to be quicker
            using (var client = new NetworkDriver(new IPCNetworkInterface(), settings))
            {
                var hostAddress = NetworkEndPoint.LoopbackIpv4.WithPort(1);
                client.Connect(hostAddress);

                // Wait past the connect timeout so there will be unanswered connect requests
                client.ScheduleUpdate().Complete();
                client.ScheduleUpdate().Complete();

                using (var host = new LocalDriverHelper(hostAddress))
                {
                    host.Host();

                    // Now give the next connect attempt time to happen
                    // TODO: Would be better to be able to see internal state here and explicitly wait until next connect attempt happens
                    //client.ScheduleUpdate().Complete();

                    host.Assert_GotConnectionRequest(client.LocalEndPoint(), true);

                    // Wait for the client to get the connect event back
                    for (int i = 0; i < 2; ++i)
                    {
                        client.ScheduleUpdate().Complete();
                        eventType = client.PopEvent(out connection, out reader);
                        if (eventType != NetworkEvent.Type.Empty)
                            break;
                    }

                    Assert.AreEqual(NetworkEvent.Type.Connect, eventType);
                }
            }
        }

        [Test]
        public void DisconnectFromARemoteEndPoint()
        {
            using (var host = new LocalDriverHelper(default(NetworkEndPoint)))
            using (var driver = new NetworkDriver(new IPCNetworkInterface()))
            {
                host.Host();

                // Need to be connected in order to be able to send a disconnect packet.
                NetworkConnection connectionId = driver.Connect(host.EndPoint);
                Assert.True(connectionId != default(NetworkConnection));
                driver.ScheduleUpdate().Complete();

                var local = driver.LocalEndPoint();
                host.Assert_GotConnectionRequest(local, true);

                NetworkConnection con;
                DataStreamReader slice;
                // Pump so we get the accept message back.
                driver.ScheduleUpdate().Complete();
                Assert.AreEqual(NetworkEvent.Type.Connect, driver.PopEvent(out con, out slice));
                driver.Disconnect(connectionId);
                driver.ScheduleUpdate().Complete();

                host.Assert_GotDisconnectionRequest(local);
            }
        }

        [Test]
        public void DisconnectTimeoutOnServer()
        {
            var settings = new NetworkSettings();
            settings.WithNetworkConfigParameters(disconnectTimeoutMS: 40, fixedFrameTimeMS: 10);
            using (var host = new LocalDriverHelper(default(NetworkEndPoint), settings))
            using (var client = new NetworkDriver(new IPCNetworkInterface(), settings))
            {
                NetworkConnection id;
                NetworkEvent.Type popEvent = NetworkEvent.Type.Empty;
                DataStreamReader reader;
                byte reason = 0;

                host.Host();

                client.Connect(host.EndPoint);
                client.ScheduleUpdate().Complete();
                host.Assert_GotConnectionRequest(client.LocalEndPoint(), true);

                // Host sends stuff but gets nothing back, until disconnect timeout happens
                for (int frm = 0; frm < 10; ++frm)
                {
                    if (host.m_LocalDriver.BeginSend(NetworkPipeline.Null, host.ClientConnections[0], out var stream) == 0)
                    {
                        for (int i = 0; i < 100; i++)
                            stream.WriteByte((byte)i);

                        host.m_LocalDriver.EndSend(stream);
                    }
                    if ((popEvent = host.m_LocalDriver.PopEvent(out id, out reader)) != NetworkEvent.Type.Empty)
                    {
                        reason = (reader.IsCreated && reader.Length > 0) ? reason = reader.ReadByte() : (byte)0;
                        break;
                    }
                    host.Update();
                }
                Assert.AreEqual(NetworkEvent.Type.Disconnect, popEvent);
                Assert.AreEqual((byte)DisconnectReason.Timeout, reason);
            }
        }

        [Test]
        public void DisconnectByRemote()
        {
            using (var host = new LocalDriverHelper(default(NetworkEndPoint)))
            using (var client = new NetworkDriver(new IPCNetworkInterface()))
            {
                host.Host();
                var popEvent = NetworkEvent.Type.Empty;
                var c = client.Connect(host.EndPoint);

                client.ScheduleUpdate().Complete();
                host.Assert_GotConnectionRequest(client.LocalEndPoint(), true);

                byte reason = 0;
                DataStreamReader reader;
                for (int frm = 0; frm < 10; ++frm)
                {
                    if (c.GetState(client) == NetworkConnection.State.Connected) c.Disconnect(client);

                    if ((popEvent = host.m_LocalDriver.PopEvent(out var id, out reader)) != NetworkEvent.Type.Empty)
                    {
                        reason = (reader.IsCreated && reader.Length > 0) ? reason = reader.ReadByte() : (byte)0;
                        break;
                    }
                    host.Update();
                    client.ScheduleUpdate().Complete();
                }
                Assert.AreEqual(NetworkEvent.Type.Disconnect, popEvent);
                Assert.AreEqual((byte)DisconnectReason.ClosedByRemote, reason);
            }
        }

        [Test]
        public void DisconnectByMaxConnectionAttempts()
        {
            var settings = new NetworkSettings();
            settings.WithNetworkConfigParameters(maxConnectAttempts: 1, fixedFrameTimeMS: 10, connectTimeoutMS: 25);

            using (var host = new LocalDriverHelper(default(NetworkEndPoint)))
            using (var client = new NetworkDriver(new IPCNetworkInterface(), settings))
            {
                host.Host();
                var popEvent = NetworkEvent.Type.Empty;
                var c = client.Connect(host.EndPoint);
                client.ScheduleUpdate().Complete();

                byte reason = 0;
                var reader = default(DataStreamReader);
                for (int frm = 0; frm < 10; ++frm)
                {
                    if ((popEvent = client.PopEvent(out var id, out reader)) != NetworkEvent.Type.Empty)
                    {
                        reason = (reader.IsCreated && reader.Length > 0) ? reason = reader.ReadByte() : (byte)0;
                        break;
                    }
                    client.ScheduleUpdate().Complete();
                }
                Assert.AreEqual(NetworkEvent.Type.Disconnect, popEvent);
                Assert.AreEqual((byte)DisconnectReason.MaxConnectionAttempts, reason);
            }
        }

        [Test]
        public void SendDataToRemoteEndPoint()
        {
            using (var host = new LocalDriverHelper(default))
            {
                host.Host();
                var driver = new NetworkDriver(new IPCNetworkInterface());

                // Need to be connected in order to be able to send a disconnect packet.
                NetworkConnection connectionId = driver.Connect(host.EndPoint);
                Assert.True(connectionId != default(NetworkConnection));
                driver.ScheduleUpdate().Complete();
                var local = driver.LocalEndPoint();
                host.Assert_GotConnectionRequest(local, true);

                NetworkConnection con;
                DataStreamReader slice;
                // Pump so we get the accept message back.
                driver.ScheduleUpdate().Complete();
                Assert.AreEqual(NetworkEvent.Type.Connect, driver.PopEvent(out con, out slice));

                var data = Encoding.ASCII.GetBytes("data to send");
                if (driver.BeginSend(NetworkPipeline.Null, connectionId, out var stream) == 0)
                {
                    stream.WriteBytes(new NativeArray<byte>(data, Allocator.Temp));
                    driver.EndSend(stream);
                }
                driver.ScheduleUpdate().Complete();

                host.Assert_GotDataRequest(local, data);

                driver.Dispose();
            }
        }

        [Test]
        public void HandleEventsFromSpecificEndPoint()
        {
            using (var host = new LocalDriverHelper(default))
            using (var client0 = new LocalDriverHelper(default))
            using (var client1 = new LocalDriverHelper(default))
            {
                host.Host();
                client0.Connect(host.EndPoint);
                client1.Connect(host.EndPoint);

                host.Assert_PopEventForConnection(client0.Connection, NetworkEvent.Type.Empty);
                host.Assert_PopEventForConnection(client1.Connection, NetworkEvent.Type.Empty);

                host.Update();

                var clientConnectionId0 = host.Accept();
                Assert.True(clientConnectionId0 != default(NetworkConnection));
                var clientConnectionId1 = host.Accept();
                Assert.True(clientConnectionId1 != default(NetworkConnection));

                client1.Update();
                client1.Assert_PopEventForConnection(client1.Connection, NetworkEvent.Type.Connect);

                client0.Update();
                client0.Assert_PopEventForConnection(client0.Connection, NetworkEvent.Type.Connect);
            }
        }

        [Test]
        public void HandleEventsFromAnyEndPoint()
        {
            using (var host = new LocalDriverHelper(default))
            using (var client0 = new LocalDriverHelper(default))
            using (var client1 = new LocalDriverHelper(default))
            {
                host.Host();
                client0.Connect(host.EndPoint);
                client1.Connect(host.EndPoint);

                host.Assert_PopEventForConnection(client0.Connection, NetworkEvent.Type.Empty);
                host.Assert_PopEventForConnection(client1.Connection, NetworkEvent.Type.Empty);

                host.Update();

                var clientConnectionId0 = host.Accept();
                Assert.True(clientConnectionId0 != default(NetworkConnection));
                var clientConnectionId1 = host.Accept();
                Assert.True(clientConnectionId1 != default(NetworkConnection));

                NetworkConnection id;

                client1.Update();
                client1.Assert_PopEvent(out id, NetworkEvent.Type.Connect);
                Assert.True(id == client1.Connection);

                client0.Update();
                client0.Assert_PopEvent(out id, NetworkEvent.Type.Connect);
                Assert.True(id == client0.Connection);
            }
        }

        [Test]
        public void DiscardEventsForNotAcceptedConnections()
        {
            using (var host = new LocalDriverHelper(default))
            using (var client0 = new LocalDriverHelper(default))
            {
                host.Host();
                client0.Connect(host.EndPoint);

                NetworkConnection clientsideConnection;
                NetworkConnection serversideNetworkConnection;

                host.Assert_PopEvent(out serversideNetworkConnection, NetworkEvent.Type.Empty);
                Assert.AreEqual(default(NetworkConnection), serversideNetworkConnection);

                host.Update();
                client0.Update();
                client0.Assert_PopEvent(out clientsideConnection, NetworkEvent.Type.Connect);
                Assert.AreEqual(client0.Connection, clientsideConnection);
                Assert.AreEqual(true, clientsideConnection.IsCreated);
                Assert.AreEqual(NetworkConnection.State.Connected, clientsideConnection.GetState(client0.m_LocalDriver));

                //Client has a connection from its perspective, host has sent back Connection Accept packet, but user-level code on host hasn't technically Accept()ed it yet
                byte[] testBytesDiscarded = Encoding.ASCII.GetBytes("this DataRequest event should be dropped");
                if (client0.m_LocalDriver.BeginSend(NetworkPipeline.Null, clientsideConnection, out var writer) == 0)
                {
                    writer.WriteBytes(new NativeArray<byte>(testBytesDiscarded, Allocator.Temp));
                    client0.m_LocalDriver.EndSend(writer);
                }

                client0.Update();
                host.Update();

                //Host hasn't Accepted connection yet, PopEvent should discard ALL events from non-accepted connections
                //in this scenario, this yields an empty event

                //Temporarily making a handle that'd be identical to what the first call to Accept() WOULD yield
                var fakeFirstNetworkConnectionToBeAccepted = new NetworkConnection
                {
                    m_NetworkId = 0,
                    m_NetworkVersion = 1
                };

                //Internal serverside queue has the 1 Data event sitting in its queue
                Assert.AreEqual(1, host.m_LocalDriver.GetEventQueueSizeForConnection(fakeFirstNetworkConnectionToBeAccepted));
                Assert.AreEqual(0, host.m_LocalDriver.GetEventQueueSizeForConnection(default(NetworkConnection)));

                //PopEvent discards that and moves onto the next item; in this case, it was the only item, so it's empty
                host.Assert_PopEvent(out serversideNetworkConnection, NetworkEvent.Type.Empty);

                //Internal queue is now size=0
                Assert.AreEqual(0, host.m_LocalDriver.GetEventQueueSizeForConnection(fakeFirstNetworkConnectionToBeAccepted));
                Assert.AreEqual(0, host.m_LocalDriver.GetEventQueueSizeForConnection(default(NetworkConnection)));

                //The actual NetworkConnection handle returned by PopEvent is still the default invalid handle
                Assert.AreEqual(default(NetworkConnection), serversideNetworkConnection);

                //Write more data
                byte[] testBytes = Encoding.ASCII.GetBytes("this DataRequest event should be processed");
                if (client0.m_LocalDriver.BeginSend(NetworkPipeline.Null, clientsideConnection, out writer) == 0)
                {
                    writer.WriteBytes(new NativeArray<byte>(testBytes, Allocator.Temp));
                    client0.m_LocalDriver.EndSend(writer);
                }
                client0.Update();

                //Finally have the host accept
                //Verify our earlier fake networkConnection would be equivalent to what the PopEvent method returns
                Assert.AreEqual(fakeFirstNetworkConnectionToBeAccepted, host.Accept());
                host.Update();

                Assert.AreEqual(1, host.m_LocalDriver.GetEventQueueSizeForConnection(fakeFirstNetworkConnectionToBeAccepted));
                Assert.AreEqual(0, host.m_LocalDriver.GetEventQueueSizeForConnection(default(NetworkConnection)));

                //Host should see the second data request now, the non-discarded one that was added prior to the Accept() call
                //This illustrates that discarding Data events on non-Accepted connections happens at PopEvent time, not at push time
                host.Assert_PopEvent(out serversideNetworkConnection, NetworkEvent.Type.Data);

                Assert.AreEqual(0, host.m_LocalDriver.GetEventQueueSizeForConnection(fakeFirstNetworkConnectionToBeAccepted));
                Assert.AreEqual(0, host.m_LocalDriver.GetEventQueueSizeForConnection(serversideNetworkConnection));
                Assert.AreEqual(0, host.m_LocalDriver.GetEventQueueSizeForConnection(default(NetworkConnection)));

                //Verify our earlier fake networkConnection was equivalent to what the PopEvent method returns
                Assert.AreEqual(fakeFirstNetworkConnectionToBeAccepted, serversideNetworkConnection);
            }
        }

        [Test]
        public void ReceiverIgnoresSenderDataPacketsAfterDisconnect()
        {
            using (var host = new LocalDriverHelper(default))
            using (var client0 = new LocalDriverHelper(default))
            {
                host.Host();
                client0.Connect(host.EndPoint);

                client0.Update();
                host.Update();
                client0.Update();
                client0.Assert_PopEvent(out var clientToHostConnection, NetworkEvent.Type.Connect);

                var hostToClientConnection = host.Accept();

                //Disconnect client, but don't send disconnect packet yet
                host.m_LocalDriver.Disconnect(hostToClientConnection);

                //Client still thinks it's connected and is able to send data
                byte[] testBytesDiscarded = Encoding.ASCII.GetBytes("host should ignore this Data packet");
                if (client0.m_LocalDriver.BeginSend(NetworkPipeline.Null, clientToHostConnection, out var writer) == 0)
                {
                    writer.WriteBytes(new NativeArray<byte>(testBytesDiscarded, Allocator.Temp));
                    client0.m_LocalDriver.EndSend(writer);
                }

                client0.Update();
                host.Update();

                host.Assert_PopEventForConnection(hostToClientConnection, NetworkEvent.Type.Empty);
            }
        }

        [Test]
        public void FillInternalBitStreamBuffer()
        {
            const int k_InternalBufferSize = 1000;
            const int k_PacketCount = 21; // Exactly enough to fill the receive buffer + 1 too much
            const int k_PacketSize = 50;
            const int k_PacketHeaderSize = UdpCHeader.Length; // The header also goes to the buffer
            const int k_PayloadSize = k_PacketSize - k_PacketHeaderSize;

            var hostSettings = new NetworkSettings();
            hostSettings.WithDataStreamParameters(size: k_InternalBufferSize);

            using (var host = new NetworkDriver(new IPCNetworkInterface(), hostSettings))
            using (var client = new NetworkDriver(new IPCNetworkInterface()))
            {
                host.Bind(NetworkEndPoint.LoopbackIpv4);

                host.Listen();

                NetworkConnection connectionId = client.Connect(host.LocalEndPoint());

                client.ScheduleUpdate().Complete();
                host.ScheduleUpdate().Complete();

                NetworkConnection poppedId;
                DataStreamReader reader;
                host.Accept();

                client.ScheduleUpdate().Complete();

                var retval = client.PopEvent(out poppedId, out reader);
                Assert.AreEqual(retval, NetworkEvent.Type.Connect);

                var dataBlob = new Dictionary<int, byte[]>();
                for (int i = 0; i < k_PacketCount; ++i)
                {
                    // Scramble each packet contents so you can't match reading the same data twice as success
                    dataBlob.Add(i, Encoding.ASCII.GetBytes(Utilities.Random.String(k_PayloadSize)));
                }

                for (int i = 0; i < k_PacketCount; ++i)
                {
                    if (client.BeginSend(NetworkPipeline.Null, connectionId, out var stream) == 0)
                    {
                        stream.WriteBytes(new NativeArray<byte>(dataBlob[i], Allocator.Temp));
                        client.EndSend(stream);
                    }
                }

                // Process the pending events
                client.ScheduleUpdate().Complete();
                host.ScheduleUpdate().Complete();

                for (int i = 0; i < k_PacketCount; ++i)
                {
                    retval = host.PopEvent(out poppedId, out reader);

                    if (i == k_PacketCount - 1)
                    {
                        Assert.AreEqual(retval, NetworkEvent.Type.Empty);
                        Assert.IsFalse(reader.IsCreated);
                        host.ScheduleUpdate().Complete();
                        retval = host.PopEvent(out poppedId, out reader);
                    }

                    Assert.AreEqual(NetworkEvent.Type.Data, retval);
                    Assert.AreEqual(reader.Length, k_PayloadSize);

                    for (int j = 0; j < k_PayloadSize; ++j)
                    {
                        Assert.AreEqual(dataBlob[i][j], reader.ReadByte());
                    }
                }
            }
        }

        static void SendAndReceiveMessage(NetworkDriver serverDriver, NetworkDriver clientDriver)
        {
            DataStreamReader stream;

            var serverEndpoint = NetworkEndPoint.Parse("127.0.0.1", (ushort)Random.Range(2000, 65000));
            serverDriver.Bind(serverEndpoint);
            serverDriver.Listen();

            var clientToServerId = clientDriver.Connect(serverEndpoint);
            clientDriver.ScheduleFlushSend(default).Complete();

            NetworkConnection serverToClientId = default(NetworkConnection);
            // Retry a few times since the network might need some time to process
            for (int i = 0; i < 10 && serverToClientId == default(NetworkConnection); ++i)
            {
                clientDriver.ScheduleUpdate().Complete();
                serverDriver.ScheduleUpdate().Complete();

                serverToClientId = serverDriver.Accept();
            }

            Assert.That(serverToClientId != default(NetworkConnection));

            clientDriver.ScheduleUpdate().Complete();

            var eventId = clientDriver.PopEventForConnection(clientToServerId, out stream);
            Assert.That(eventId == NetworkEvent.Type.Connect, $"Expected Connect but got {eventId} using {backend}");

            //52 bytes of data below:
            int testInt = 100;
            float testFloat = 555.5f;
            byte[] testByteArray = Encoding.ASCII.GetBytes("Some bytes blablabla 1111111111111111111");

            var sentBytes = 0;
            if (clientDriver.BeginSend(NetworkPipeline.Null, clientToServerId, out var clientSendData) == 0)
            {
                clientSendData.WriteInt(testInt);
                clientSendData.WriteFloat(testFloat);
                clientSendData.WriteInt(testByteArray.Length);
                clientSendData.WriteBytes(new NativeArray<byte>(testByteArray, Allocator.Temp));
                sentBytes = clientDriver.EndSend(clientSendData);
            }

            Assert.AreEqual(clientSendData.Length, sentBytes);

            clientDriver.ScheduleUpdate().Complete();
            serverDriver.ScheduleUpdate().Complete();

            DataStreamReader serverReceiveStream;
            eventId = serverDriver.PopEventForConnection(serverToClientId, out serverReceiveStream);

            Assert.True(eventId == NetworkEvent.Type.Data);
            var receivedInt = serverReceiveStream.ReadInt();
            var receivedFloat = serverReceiveStream.ReadFloat();
            var byteArrayLength = serverReceiveStream.ReadInt();
            var receivedBytes = new NativeArray<byte>(byteArrayLength, Allocator.Temp);
            serverReceiveStream.ReadBytes(receivedBytes);

            Assert.True(testInt == receivedInt);
            Assert.That(Mathf.Approximately(testFloat, receivedFloat));
            Assert.AreEqual(testByteArray, receivedBytes);
        }

        //Note for the below 3 tests:
        //SendAndReceiveMessage() as currently written sends 52 + sizeof(UdpCHeader) bytes.
        //If the size of UdpCHeader goes over 12 bytes in the future (currently 10 bytes at time of writing),
        //NetworkDataStreamParameter.size in these below tests will need to be increased accordingly.
        [Test]
        public void SendAndReceiveMessage_RealNetwork()
        {
            using (var serverDriver = NetworkDriver.Create())
            using (var clientDriver = NetworkDriver.Create())
            {
                SendAndReceiveMessage(serverDriver, clientDriver);
            }
        }

        [Test]
        public void SendAndReceiveMessage()
        {
            using (var serverDriver = new NetworkDriver(new IPCNetworkInterface()))
            using (var clientDriver = new NetworkDriver(new IPCNetworkInterface()))
            {
                SendAndReceiveMessage(serverDriver, clientDriver);
            }
        }
    }
}
