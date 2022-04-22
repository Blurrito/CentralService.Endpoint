using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.DTO.Authentication;
using CentralService.Endpoint.DTO.User;
using CentralService.Endpoint.DTO.Common;

namespace CentralService.Endpoint.Interfaces.Clients
{
    public interface IAuthenticationClient : IDisposable
    {
        Task GenerateChallenge(NasToken Token);
        Task<ApiResponse?> GetChallenge(string Address);
        Task<ApiResponse?> ValidateChallengeResponse(string Address, ChallengeResult Result);
        Task<ApiResponse?> GetMatchmakingChallenge(int SessionId, string GameName, string Address, string Port);
        Task<ApiResponse?> ValidateMatchmakingChallengeResult(MatchmakingChallengeResult Result);
    }
}
