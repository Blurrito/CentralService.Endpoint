using CentralService.Endpoint.DTO.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct Error
    {
        public string error => string.Empty;
        public int err { get; set; }
        public string fatal { get; set; }
        public string errmsg { get; set; }
        public int id { get; set; }

        public Error(ApiError Error, int MessageId)
        {
            err = Error.ErrorCode;
            fatal = Error.IsFatal ? string.Empty : null;
            errmsg = Error.Reason;
            id = MessageId;
        }
    }
}
