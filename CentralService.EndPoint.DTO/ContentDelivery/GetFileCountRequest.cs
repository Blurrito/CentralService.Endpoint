using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.ContentDelivery
{
    public struct GetFileCountRequest
    {
        public string GameCode { get; set; }
        public List<string> Attributes { get; set; }

        public GetFileCountRequest(string GameCode, List<string> Attributes)
        {
            this.GameCode = GameCode;
            this.Attributes = Attributes;
        }
    }
}
