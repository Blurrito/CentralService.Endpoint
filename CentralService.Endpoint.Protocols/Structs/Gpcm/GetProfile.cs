using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct GetProfile
    {
        public string getprofile => string.Empty;
        public int sesskey { get; set; }
        public int profileid { get; set; }
        public int id { get; set; }
    }
}
