using CentralService.Endpoint.DTO.Matchmaking;
using CentralService.Factories.Servers.EndPoint;
using CentralService.Factories.Servers.Enums;
using CentralService.Endpoint.Interfaces.Protocols;
using CentralService.Endpoint.Interfaces.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CentralService.EndPoint.Presentation
{
    public class Client
    {
        public byte[] Buffer { get; set; }
        public int BufferSize => Buffer.Length;
        public System.Net.EndPoint ClientEndPoint { get; set; }
        public IPAddress Address => ((IPEndPoint)ClientEndPoint).Address;
        public short Port => (short)((IPEndPoint)ClientEndPoint).Port;
        public Socket Listener { get; }

        public Client(Socket ListenerSocket, int BufferSize = 2048)
        {
            Listener = ListenerSocket;
            Buffer = new byte[BufferSize];
        }

        public Client(IPEndPoint ClientEndPoint, Socket ListenerSocket, byte[] Buffer)
        {
            this.ClientEndPoint = ClientEndPoint;
            Listener = ListenerSocket;
            this.Buffer = Buffer;
        }
    }

    public class UdpListener : IListener
    {
        public bool Running => _Running;
        public ServerTypes ServerType { get; }
        public string DomainAddress { get; set; }

        private bool _Running = false;
        private IPEndPoint _ListenerEndPoint;
        private System.Net.EndPoint _RemoteEndPoint;
        private Socket _Listener;
        private readonly object _ListenerLock = new object();

        private Task _SendPendingMessagesTask;
        private Task _ReceiveIncomingMessagesTask;

        public UdpListener(IPAddress Address, int Port, string DomainAddress, ServerTypes ServerType)
        {
            this.DomainAddress = DomainAddress;
            this.ServerType = ServerType;
            _RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            if (Address != null)
            {
                _ListenerEndPoint = new IPEndPoint(Address, Port);
                _Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _Listener.Bind(_ListenerEndPoint);
            }
            Console.WriteLine($"{ DomainAddress } - Successfully created on { Address }, port { Port }.");
        }

        public void Start()
        {
            if (!Running)
            {
                _Running = true;
                try
                {
                    _ReceiveIncomingMessagesTask = new Task(async () => await ReceiveIncomingMessages());
                    _SendPendingMessagesTask = new Task(async () => await SendPendingMessages());
                    _ReceiveIncomingMessagesTask.Start();
                    _SendPendingMessagesTask.Start();
                    Console.WriteLine($"{ DomainAddress } - Successfully started.");
                }
                catch
                {

                }
            }
        }

        public void Stop()
        {
            if (Running)
            {
                _Running = false;
                if (_SendPendingMessagesTask != null && _ReceiveIncomingMessagesTask != null)
                    while (!_SendPendingMessagesTask.IsCompleted || !_ReceiveIncomingMessagesTask.IsCompleted)
                        Thread.Sleep(100);
            }
            Console.WriteLine($"{ DomainAddress } - Successfully stopped.");
        }

        private ManualResetEvent _SenderConnected = new ManualResetEvent(false);

        private async Task SendPendingMessages()
        {
            using (IUdpServer Server = UdpServerFactory.GetUdpServer(ServerType))
            {
                while (Running)
                {
                    List<MatchmakingMessage> PendingMessages = Server.GetPendingMessages();
                    foreach (MatchmakingMessage Message in PendingMessages)
                    {
                        Client Client = new Client(new IPEndPoint(Message.Address, Message.Port), _Listener, Message.Data);
                        _Listener.BeginSendTo(Client.Buffer, 0, Client.BufferSize, 0, Client.ClientEndPoint, new AsyncCallback(BeginSendToCallback), Client);
                    }
                    await Task.Delay(1000);
                }
            }
        }

        private async Task ReceiveIncomingMessages()
        {
            using (IUdpServer Server = UdpServerFactory.GetUdpServer(ServerType))
            {
                while (Running)
                {
                    _SenderConnected.Reset();

                    Client Client = new Client(_Listener);
                    _Listener.BeginReceiveFrom(Client.Buffer, 0, Client.BufferSize, 0, ref _RemoteEndPoint, new AsyncCallback(BeginReceiveCallback), Client);

                    _SenderConnected.WaitOne();
                }
            }
        }

        private async void BeginReceiveCallback(IAsyncResult Callback)
        {
            _SenderConnected.Set();
            try
            {
                Client Client = (Client)Callback.AsyncState;
                System.Net.EndPoint RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                lock (_ListenerLock)
                    Client.Listener.EndReceiveFrom(Callback, ref RemoteEndPoint);
                Client.ClientEndPoint = RemoteEndPoint;

                byte[] Response;
                using (IUdpServer Server = UdpServerFactory.GetUdpServer(ServerType))
                    Response = await Server.HandleClient(Client.Address, Client.Port, Client.Buffer);
                Client.Buffer = Response;

                if (Client.Buffer != null)
                    Client.Listener.BeginSendTo(Client.Buffer, 0, Client.BufferSize, 0, Client.ClientEndPoint, new AsyncCallback(BeginSendToCallback), Client);
            }
            catch
            {

            }
        }

        private void BeginSendToCallback(IAsyncResult Callback)
        {
            try
            {
                Client Client = (Client)Callback.AsyncState;
                lock (_ListenerLock)
                    Client.Listener.EndSendTo(Callback);
            }
            catch
            {

            }
        }
    }
}
