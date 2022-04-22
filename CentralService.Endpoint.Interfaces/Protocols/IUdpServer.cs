using CentralService.Endpoint.DTO.Matchmaking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Interfaces.Protocols
{
    public interface IUdpServer : IDisposable
    {
        Task<byte[]> HandleClient(IPAddress Address, short Port, byte[] Data);
        List<MatchmakingMessage> GetPendingMessages();
    }
}
