using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.DTO.Authentication;
using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.User;
using CentralService.Endpoint.Interfaces.Clients;
using Newtonsoft.Json;

namespace CentralService.Endpoint.Client
{
    public class AuthenticationClient : BaseClient, IAuthenticationClient
    {
        public string ApiPath { get; }

        public AuthenticationClient(string ApiPath) : base(Properties.Resources.AuthenticationApiUrl) => this.ApiPath = ApiPath;

        public async Task GenerateChallenge(NasToken Token) => await Create($"{ ApiPath }/generateuserchallenge", Token);

        public async Task<ApiResponse?> GetChallenge(string Address)
        {
            HttpResponseMessage Response = await Get($"{ ApiPath }/getuserchallenge?Address={ Address }");
            return await ProcessResponse<ApiResponse>(Response);
        }

        public async Task<ApiResponse?> ValidateChallengeResponse(string Address, ChallengeResult Result)
        {
            HttpResponseMessage Response = await Create($"{ ApiPath }/validateuserchallengeresponse?Address={ Address }", Result);
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<ChallengeProof>(ResponseObject);
        }

        public async Task<ApiResponse?> GetMatchmakingChallenge(int SessionId, string GameName, string Address, string Port)
        {
            HttpResponseMessage Response = await Get($"{ ApiPath }/getmatchmakingchallenge?SessionId={ SessionId }&GameName={ GameName }&Address={ Address }&Port={ Port }");
            return await ProcessResponse<ApiResponse>(Response);
        }

        public async Task<ApiResponse?> ValidateMatchmakingChallengeResult(MatchmakingChallengeResult Result)
        {
            HttpResponseMessage Response = await Create($"{ ApiPath }/validatematchmakingchallengeresult", Result);
            return await ProcessResponse<ApiResponse>(Response);
        }

        private ApiResponse? DeserializeResponseContent<TType>(ApiResponse? Response)
        {
            if (Response.HasValue)
                if (Response.Value.Content != null)
                {
                    string ContentString = Response.Value.Content.ToString();
                    if (Response.Value.Success)
                        return new ApiResponse(true, JsonConvert.DeserializeObject<TType>(ContentString));
                    return new ApiResponse(false, JsonConvert.DeserializeObject<ApiError>(ContentString));
                }
            return Response;
        }

        public void Dispose() { }
    }
}
