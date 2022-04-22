using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct StatusUpdate
    {
        public int status { get; set; }
        public int sesskey { get; set; }
        public string statstring { get; set; }
        public string locstring { get; set; }
    }
}
