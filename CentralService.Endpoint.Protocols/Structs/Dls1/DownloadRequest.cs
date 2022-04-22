using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Dls1
{
    public struct DownloadRequest
    {
        public string gamecd { get; set; }
        public string rhgamecd { get; set; }
        public string passwd { get; set; }
        public string token { get; set; }
        public long userid { get; set; }
        public string macadr { get; set; }
        public string action { get; set; }
        public string apinfo { get; set; }

        public string attr1 { get; set; }
        public string attr2 { get; set; }
        public string attr3 { get; set; }
        public int offset { get; set; }
        public int num { get; set; }
        public string contents { get; set; }
    }
}
