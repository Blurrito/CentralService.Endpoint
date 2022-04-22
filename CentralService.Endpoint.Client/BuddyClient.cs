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
    public class BuddyClient : BaseClient, IBuddyClient
    {
        public string ApiPath { get; }

        public BuddyClient(string ApiPath) : base(Properties.Resources.UserApiUrl) => this.ApiPath = ApiPath;

        public async Task<ApiResponse?> GetBuddyList(int SenderId)
        {
            HttpResponseMessage Response = await Get($"{ ApiPath }/getbuddylist?SenderId={ SenderId }");
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<List<Buddy>>(ResponseObject);
        }

        public async Task<ApiResponse?> GetIncomingRequests(int RecipientId)
        {
            HttpResponseMessage Response = await Get($"{ ApiPath }/getincomingrequests?RecipientId={ RecipientId }");
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<List<Buddy>>(ResponseObject);
        }

        public async Task<ApiResponse?> AddBuddy(string GameCode, Buddy Buddy)
        {
            HttpResponseMessage Response = await Create($"{ ApiPath }/addbuddy?GameCode={ GameCode }", Buddy);
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<int>(ResponseObject);
        }

        public async Task UpdateBuddy(Buddy Buddy) => await Update($"{ ApiPath }/updatebuddy", Buddy);

        public async Task DeleteBuddy(int BuddyId) => await Delete($"{ ApiPath }/deletebuddy?BuddyId={ BuddyId }");

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
