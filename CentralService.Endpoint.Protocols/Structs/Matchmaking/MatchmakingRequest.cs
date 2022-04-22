using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Matchmaking
{
    public struct MatchmakingRequest
    {
        public byte RequestType { get; set; }
        public int ClientId { get; set; }
        public byte[] Data { get; set; }

        public MatchmakingRequest(byte[] Request)
        {
            if (Request.Length < 5)
                throw new ArgumentException("Provided byte array is too short.", nameof(Request));
            RequestType = Request[0];
            ClientId = BitConverter.ToInt32(Request, 1);
            Data = new byte[Request.Length - 5];
            Array.Copy(Request, 5, Data, 0, Data.Length);
        }
    }
}
