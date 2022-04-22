using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Authentication
{
    public struct MatchmakingChallengeResult
    {
        public int SessionId { get; set; }
        public string ChallengeResult { get; set; }

        public MatchmakingChallengeResult(int SessionId, string ChallengeResult)
        {
            this.SessionId = SessionId;
            this.ChallengeResult = ChallengeResult;
        }
    }
}
