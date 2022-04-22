using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Authentication
{
    public struct ChallengeProof
    {
        public string Address { get; set; }
        public int DeviceProfileId { get; set; }
        public int GameProfileId { get; set; }
        public string GameCode { get; set; }
        public string RegionalGameCode { get; set; }
        public string UniqueNickname { get; set; }
        public string Proof { get; set; }
        public string LoginToken { get; set; }
    }
}
