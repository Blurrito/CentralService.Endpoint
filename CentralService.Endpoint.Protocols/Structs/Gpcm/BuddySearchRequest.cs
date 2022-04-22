using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct BuddySearchRequest
    {
        public string search => string.Empty;
        public int sesskey { get; set; }
        public int profileid { get; set; }
        public int namespaceid { get; set; }
        public string lastname { get; set; }
        public string gamename { get; set; }
    }
}
