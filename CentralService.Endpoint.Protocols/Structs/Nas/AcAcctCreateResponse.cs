using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Nas
{
    public struct AcAcctCreateResponse
    {
        public string locator { get; set; }
        public string retry { get; set; }
        public string returncd { get; set; }
        public int userid { get; set; }

        public AcAcctCreateResponse(int UserId)
        {
            locator = "gamespy.com";
            retry = "0";
            returncd = "002";
            userid = UserId;
        }
    }
}
