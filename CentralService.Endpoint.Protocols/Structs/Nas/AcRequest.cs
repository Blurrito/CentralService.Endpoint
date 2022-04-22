using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Nas
{
    public struct AcRequest
    {
        public string action { get; set; }
        public string gsbrcd { get; set; }
        public string sdkver { get; set; }
        public long userid { get; set; }
        public string passwd { get; set; }
        public string bssid { get; set; }
        public string apinfo { get; set; }
        public string gamecd { get; set; }
        public byte makercd { get; set; }
        public byte unitcd { get; set; }
        public string macadr { get; set; }
        public byte lang { get; set; }
        public short birth { get; set; }
        public string devtime { get; set; }
        public string devname { get; set; }
        public string ingamesn { get; set; }
    }
}
