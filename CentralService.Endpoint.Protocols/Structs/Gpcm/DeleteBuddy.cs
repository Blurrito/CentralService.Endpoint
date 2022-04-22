using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct DeleteBuddy
    {
        public string delbuddy => string.Empty;
        public int sesskey { get; set; }
        public int delprofileid { get; set; }
    }
}
