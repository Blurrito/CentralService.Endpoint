using CentralService.Endpoint.DTO.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct LoggedIn
    {
        public string lc => "2";
        public int sesskey { get; set; }
        public string proof { get; set; }
        public long userid { get; set; }
        public long profileid { get; set; }
        public string uniquenick { get; set; }
        public string lt { get; set; }
        public int id { get; set; }

        public LoggedIn(ChallengeProof Proof, int SessionKey, int MessageId)
        {
            sesskey = SessionKey;
            proof = Proof.Proof;
            userid = Proof.DeviceProfileId;
            profileid = Proof.GameProfileId;
            uniquenick = Proof.UniqueNickname;
            lt = Proof.LoginToken;
            id = MessageId;
        }
    }
}
