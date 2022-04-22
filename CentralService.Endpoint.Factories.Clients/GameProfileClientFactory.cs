using CentralService.Endpoint.Client;
using CentralService.Endpoint.Interfaces.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Factories.Clients
{
    public static class GameProfileClientFactory
    {
        public static IGameProfileClient GetClient() => new GameProfileClient("/api/ds/game");
    }
}
