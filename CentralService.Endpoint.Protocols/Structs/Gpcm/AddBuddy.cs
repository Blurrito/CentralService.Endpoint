using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct AddBuddy
    {
        public string addbuddy => string.Empty;
        public int sesskey { get; set; }
        public int newprofileid { get; set; }
        public string reason { get; set; }
    }
}
