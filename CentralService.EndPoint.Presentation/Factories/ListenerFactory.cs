using CentralService.Endpoint.Presentation.Structs;
using CentralService.EndPoint.Presentation.Enums;
using CentralService.Endpoint.Interfaces.Protocols;
using CentralService.Endpoint.Interfaces.Listeners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.EndPoint.Presentation.Factories
{
    public static class ListenerFactory
    {
        public static IListener GetListener(ServerConfiguration Configuration)
        {
            switch (Configuration.ListenerType)
            {
                case ListenerTypes.TCP:
                    return GetTcpListener(Configuration);
                case ListenerTypes.UDP:
                    return GetUdpListener(Configuration);
                default:
                    throw new NotImplementedException();
            }
        }

        private static IListener GetTcpListener(ServerConfiguration Configuration)
        {
            TcpListener Listener = new TcpListener(IPAddress.Parse(Configuration.Address), Configuration.Port, Configuration.DomainName, Configuration.ServerType, Configuration.UseSsl);
            if (Configuration.UseSsl)
            {
                Listener.GetServerCertificate(Configuration.ServerCertificateName);
                if (Configuration.AdditionalCertificateNames != null)
                    foreach (string AdditionalCertificate in Configuration.AdditionalCertificateNames)
                        Listener.GetAdditionalCertificate(AdditionalCertificate);
            }
            return Listener;
        }

        private static IListener GetUdpListener(ServerConfiguration Configuration) => new UdpListener(IPAddress.Parse(Configuration.Address), Configuration.Port, Configuration.DomainName, Configuration.ServerType);
    }
}
