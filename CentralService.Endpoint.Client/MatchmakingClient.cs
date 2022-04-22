using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.Matchmaking;
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
    public class MatchmakingClient : BaseClient, IMatchmakingClient
    {
        public string ApiPath { get; }

        public MatchmakingClient(string ApiPath) : base(Properties.Resources.MatchmakingApiUrl) => this.ApiPath = ApiPath;

        public async Task<ApiResponse?> GetMultiplayerServers(GetServerRequest Request)
        {
            HttpResponseMessage Response = await Create($"{ ApiPath }", Request);
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<GetServerResponse>(ResponseObject);
        }

        public async Task ManageMultiplayerServer(ServerUpdate Server) => await Update($"{ ApiPath }/manageserver", Server);

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
