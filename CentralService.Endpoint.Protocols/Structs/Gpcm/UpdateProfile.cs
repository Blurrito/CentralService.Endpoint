using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct UpdateProfile
    {
        public string updatepro => string.Empty;
        public int sesskey { get; set; }
        public string nick { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string zipcode { get; set; }
        public string aim { get; set; }
        public float lon { get; set; }
        public float lat { get; set; }
        public string loc { get; set; }
    }
}
