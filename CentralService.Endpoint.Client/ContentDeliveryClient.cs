using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.ContentDelivery;
using CentralService.Endpoint.Interfaces.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Client
{
    public class ContentDeliveryClient : BaseClient, IContentDeliveryClient
    {
        public string ApiPath { get; }

        public ContentDeliveryClient(string ApiPath) : base(Properties.Resources.UserApiUrl) => this.ApiPath = ApiPath;

        public Task<ApiResponse?> GetFileContent(GetFileContentRequest Request)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse?> GetFileCount(GetFileCountRequest Request)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse?> GetFileList(GetFileListRequest Request)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
