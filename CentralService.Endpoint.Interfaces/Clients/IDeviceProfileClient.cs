using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.Authentication;
using CentralService.Endpoint.DTO.User;

namespace CentralService.Endpoint.Interfaces.Clients
{
    public interface IDeviceProfileClient : IDisposable
    {
        Task<ApiResponse?> CreateDeviceProfile(DeviceProfile Request);
        Task<ApiResponse?> Login(string Address, DeviceProfile Request);
    }
}
