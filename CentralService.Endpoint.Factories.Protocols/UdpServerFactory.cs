using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.Protocols;
using CentralService.Factories.Servers.Enums;
using CentralService.Endpoint.Interfaces.Protocols;

namespace CentralService.Factories.Servers.EndPoint
{
    public static class UdpServerFactory
    {
        public static IUdpServer GetUdpServer(ServerTypes UdpServerType)
        {
            switch (UdpServerType)
            {
                case ServerTypes.Dns:
                    return new DnsServer();
                case ServerTypes.Matchmaking:
                    return new MatchmakingServer();
                default:
                    throw new NotImplementedException($"Udp server type { UdpServerType } does not have an implementation.");
            }
        }
    }
}
