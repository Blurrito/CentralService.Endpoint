using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CentralService.Factories.Servers.EndPoint;
using CentralService.Endpoint.Interfaces.Listeners;
using CentralService.Endpoint.Interfaces.Protocols;
using CentralService.Factories.Servers.Enums;

namespace CentralService.EndPoint.Presentation
{
    public class TcpListener : System.Net.Sockets.TcpListener, IListener
    {
        public IPAddress Address { get; }
        public int Port { get; }
        public string DomainAddress { get; }
        public bool Running { get; private set; }
        public bool UseSSL { get; }
        public ServerTypes ServerType { get; }

        private X509Certificate2 _Certificate;
        private X509Certificate2Collection _CertificateChain = new X509Certificate2Collection();

        private List<Thread> _ClientThreads = new List<Thread>();
        private readonly object _ClientThreadLock = new object();

        public TcpListener(IPAddress Address, int Port, string DomainAddress, ServerTypes ServerType, bool UseSsl) : base(Address, Port)
        {
            this.Address = Address;
            this.Port = Port;
            this.DomainAddress = DomainAddress;
            this.ServerType = ServerType;
            UseSSL = UseSsl;
            Console.WriteLine($"{ DomainAddress } - Successfully created on { Address }, port { Port }.");
        }

        public void Start()
        {
            Running = true;
            base.Start();
            lock (_ClientThreadLock)
            {
                for (int i = 0; i < 1; i++)
                {
                    Thread ListenerThread = new Thread(new ThreadStart(MainLoop));
                    ListenerThread.Start();
                    _ClientThreads.Add(ListenerThread);
                }
            }
            Console.WriteLine($"{ DomainAddress } - Successfully started.");
        }

        public void Stop()
        {
            Running = false;
            lock (_ClientThreadLock)
                while (_ClientThreads.Count > 0)
                    _ClientThreads.RemoveAll(x => !x.IsAlive);
            base.Stop();
            Console.WriteLine($"{ DomainAddress } - Successfully stopped.");
        }

        private void MainLoop()
        {
            List<Task> RunningTasks = new List<Task>(128);
            while (Running)
            {
                if (Pending() && RunningTasks.Count < RunningTasks.Capacity)
                {
                    TcpClient Client = AcceptTcpClient();
                    if (Client != null)
                    {
                        Stream ClientStream = GetStream(Client);
                        if (ClientStream != null)
                        {
                            Task HandleClientTask = new Task(() =>
                            {
                                ITcpServer Server = TcpServerFactory.GetTcpServer(ServerType);
                                Server.HandleClient(Client, ClientStream);
                            });
                            HandleClientTask.Start();
                            RunningTasks.Add(HandleClientTask);
                        }
                    }
                }
                else
                    Thread.Sleep(100);
                RunningTasks.RemoveAll(x => x.IsCompleted);
            }

            while (RunningTasks.Count > 0)
                RunningTasks.RemoveAll(x => x.IsCompleted);
        }

        public void GetServerCertificate(string CertificateName, StoreName StoreName = StoreName.My, StoreLocation StoreLocation = StoreLocation.CurrentUser) => _Certificate = GetCertificate(CertificateName, StoreName, StoreLocation);

        public void GetAdditionalCertificate(string CertificateName, StoreName StoreName = StoreName.My, StoreLocation StoreLocation = StoreLocation.CurrentUser) => _CertificateChain.Add(GetCertificate(CertificateName, StoreName, StoreLocation));

        /// <summary>
        /// Get the server certificates required for the HTTPS server to run.
        /// </summary>
        /// <param name="StoreName">The store name where the certificates are located. Defaults to the personal certificate store.</param>
        /// <param name="StoreLocation">The location of the store where the certificates are locared. Defaults to the current user (Local machine requires administrative permissions).</param>
        private X509Certificate2 GetCertificate(string CertificateName, StoreName StoreName, StoreLocation StoreLocation)
        {
            X509Store Store = new X509Store(StoreName, StoreLocation);
            Store.Open(OpenFlags.ReadOnly);

            try
            {
                X509Certificate2Collection StoreCollection = Store.Certificates;
                X509Certificate2Collection FoundCertificates = StoreCollection.Find(X509FindType.FindBySubjectName, CertificateName, false);
                if (FoundCertificates.Count < 1)
                    throw new Exception($"{ DomainAddress } => Could not find a certificate with name { CertificateName }. Make sure this certificate has been added to the selected certificate store (Store name: { StoreName }, Store location: { StoreLocation }).");
                return FoundCertificates[0];
            }
            finally
            {
                Store.Close();
            }
        }

        private Stream GetStream(TcpClient Client)
        {
            try
            {
                Stream NetworkStream;
                if (UseSSL)
                {
                    SslStream EncryptedStream = new SslStream(Client.GetStream(), false);
                    EncryptedStream.AuthenticateAsServer(new SslServerAuthenticationOptions
                    {
                        ServerCertificateContext = SslStreamCertificateContext.Create(_Certificate, _CertificateChain),
                        EnabledSslProtocols = SslProtocols.Ssl3,
                        ClientCertificateRequired = false
                    });
                    NetworkStream = EncryptedStream;
                }
                else
                    NetworkStream = Client.GetStream();
                return NetworkStream;
            }
            catch (Exception Ex)
            {
                Console.Write($"{ DomainAddress } - Exception: { Ex.Message }");
                return null;
            }
        }
    }
}
