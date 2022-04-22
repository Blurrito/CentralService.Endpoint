using CentralService.Endpoint.DTO.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Nas
{
    public struct AcLoginResponse
    {
        public string challenge { get; set; }
        public string locator { get; set; }
        public string retry { get; set; }
        public string returncd { get; set; }
        public string token { get; set; }
        public string date { get; set; }

        public AcLoginResponse(string Token, string Challenge)
        {
            challenge = Challenge;
            locator = "gamespy.com";
            retry = "0";
            returncd = "001";
            token = Token;
            date = DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
