using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.Protocols;
using CentralService.Factories.Servers.Enums;
using CentralService.Endpoint.Interfaces.Protocols;

namespace CentralService.Factories.Servers.EndPoint
{
    public static class TcpServerFactory
    {
        public static ITcpServer GetTcpServer(ServerTypes Type)
        {
            switch (Type)
            {
                case ServerTypes.ConnectionTest:
                    return new ConnectionTestServer();
                case ServerTypes.Nas:
                    return new NasServer();
                case ServerTypes.Profile:
                    return new ProfileServer();
                case ServerTypes.ServerSearch:
                    return new ServerSearchServer();
                default:
                    throw new ArgumentException($"No server implementation for server type { Type }.");
            }
        }
    }
}
