using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Matchmaking
{
    public struct MatchmakingMessage
    {
        public IPAddress Address { get; set; }
        public int Port { get; set; }
        public byte[] Data { get; set; }

        public MatchmakingMessage(IPAddress Address, int Port, byte[] Data)
        {
            this.Address = Address;
            this.Port = Port;
            this.Data = Data;
        }
    }
}
