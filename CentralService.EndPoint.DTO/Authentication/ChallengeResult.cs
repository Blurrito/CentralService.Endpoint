using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Authentication
{
    public struct ChallengeResult
    {
        public string NasToken { get; set; }
        public string Challenge { get; set; }
        public string Result { get; set; }

        public ChallengeResult(string NasToken, string Challenge, string Result)
        {
            this.NasToken = NasToken;
            this.Challenge = Challenge;
            this.Result = Result;
        }
    }
}
