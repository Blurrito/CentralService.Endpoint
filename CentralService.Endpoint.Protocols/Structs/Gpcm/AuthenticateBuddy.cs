using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct AuthenticateBuddy
    {
        public string authadd => string.Empty;
        public int sesskey { get; set; }
        public int fromprofileid { get; set; }
        public string sig { get; set; }
    }
}
