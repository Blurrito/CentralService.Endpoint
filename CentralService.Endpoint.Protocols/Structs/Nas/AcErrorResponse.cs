using CentralService.Endpoint.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Nas
{
    public struct AcErrorResponse
    {
        public string locator { get; set; }
        public string retry { get; set; }
        public string returncd { get; set; }
        public string reason { get; set; }
        public string date { get; set; }

        public AcErrorResponse(ApiError Error)
        {
            locator = "gamespy.com";
            retry = "1";
            returncd = Error.ErrorCode.ToString();
            reason = Error.Reason;
            date = DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
