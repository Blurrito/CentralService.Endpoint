using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.ContentDelivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Interfaces.Clients
{
    public interface IContentDeliveryClient : IDisposable
    {
        Task<ApiResponse?> GetFileCount(GetFileCountRequest Request);
        Task<ApiResponse?> GetFileList(GetFileListRequest Request);
        Task<ApiResponse?> GetFileContent(GetFileContentRequest Request);
    }
}
