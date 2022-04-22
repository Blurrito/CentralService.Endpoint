using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct BuddyMessage
    {
        public int bm { get; set; }
        public int t { get; set; }
        public string date { get; set; }
        public string msg { get; set; }
    }
}
