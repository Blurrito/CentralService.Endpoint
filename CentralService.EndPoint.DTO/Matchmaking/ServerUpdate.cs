using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Matchmaking
{
    public struct ServerUpdate
    {
        public int SessionId { get; set; }
        public int Console { get; set; }
        public int State { get; set; }
        public string ActualAddress { get; set; }
        public List<KeyValuePair<string, string>> Properties { get; set; }

        public ServerUpdate(int SessionId, int Console, int State, string ActualAddress, List<KeyValuePair<string, string>> Properties)
        {
            this.SessionId = SessionId;
            this.Console = Console;
            this.State = State;
            this.ActualAddress = ActualAddress;
            this.Properties = Properties;
        }
    }
}
