using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.Matchmaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Interfaces.Clients
{
    public interface IMatchmakingClient : IDisposable
    {
        Task ManageMultiplayerServer(ServerUpdate Update);
        Task<ApiResponse?> GetMultiplayerServers(GetServerRequest Request);
    }
}
