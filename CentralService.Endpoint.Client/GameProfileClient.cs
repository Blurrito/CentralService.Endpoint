using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.User;
using CentralService.Endpoint.Interfaces.Clients;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Client
{
    public class GameProfileClient : BaseClient, IGameProfileClient
    {
        public string ApiPath { get; }

        public GameProfileClient(string ApiPath) : base(Properties.Resources.UserApiUrl) => this.ApiPath = ApiPath;

        public async Task<ApiResponse?> GetGameProfile(int GameProfileId)
        {
            HttpResponseMessage Response = await Get($"{ ApiPath }?GameProfileId={ GameProfileId }");
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<GameProfile>(ResponseObject);
        }

        public async Task<ApiResponse?> GetBuddyProfile(int GameProfileId, string BuddyLastName)
        {
            HttpResponseMessage Response = await Get($"{ ApiPath }/getbuddyprofile?GameProfileId={ GameProfileId }&BuddyLastName={ BuddyLastName }");
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<GameProfile>(ResponseObject);
        }

        public async Task UpdateGameProfile(int SessionKey, GameProfile Profile) => await Create($"{ ApiPath }/update?SessionKey={ SessionKey }", Profile);

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
