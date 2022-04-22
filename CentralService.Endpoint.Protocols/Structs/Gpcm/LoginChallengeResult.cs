using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct LoginChallengeResult
    {
        public string login => string.Empty;
        public string challenge { get; set; }
        public string authtoken { get; set; }
        public string response { get; set; }
        public int firewall { get; set; }
        public int port { get; set; }
        public int productid { get; set; }
        public string gamename { get; set; }
        public int namespaceid { get; set; }
        public int id { get; set; }
    }
}
