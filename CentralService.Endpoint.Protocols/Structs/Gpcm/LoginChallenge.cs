using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct LoginChallenge
    {
        public string lc => "1";
        public string challenge { get; set; }
        public int id { get; set; }

        public LoginChallenge(string Challenge)
        {
            challenge = Challenge;
            id = 1;
        }
    }
}
