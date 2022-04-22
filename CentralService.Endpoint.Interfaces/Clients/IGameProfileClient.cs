using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Interfaces.Clients
{
    public interface IGameProfileClient : IDisposable
    {
        Task<ApiResponse?> GetGameProfile(int GameProfileId);
        Task<ApiResponse?> GetBuddyProfile(int GameProfileId, string BuddyLastName);
        Task UpdateGameProfile(int SessionKey, GameProfile Profile);
    }
}
