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
    public class DeviceProfileClient : BaseClient, IDeviceProfileClient
    {
        public string ApiPath { get; }

        public DeviceProfileClient(string ApiPath) : base(Properties.Resources.UserApiUrl) => this.ApiPath = ApiPath;

        public async Task<ApiResponse?> CreateDeviceProfile(DeviceProfile Request)
        {
            HttpResponseMessage Response = await Create($"{ ApiPath }/profilecreate", Request);
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<int>(ResponseObject);
        }

        public async Task<ApiResponse?> Login(string Address, DeviceProfile Request)
        {
            HttpResponseMessage Response = await Create($"{ ApiPath }/login?Address={ Address }", Request);
            ApiResponse? ResponseObject = await ProcessResponse<ApiResponse>(Response);
            return DeserializeResponseContent<NasToken>(ResponseObject);
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
