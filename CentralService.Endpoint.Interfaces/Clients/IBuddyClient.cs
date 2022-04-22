using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Interfaces.Clients
{
    public interface IBuddyClient : IDisposable
    {
        Task<ApiResponse?> GetBuddyList(int SenderId);
        Task<ApiResponse?> GetIncomingRequests(int RecipientId);
        Task<ApiResponse?> AddBuddy(string GameCode, Buddy Buddy);
        Task UpdateBuddy(Buddy Buddy);
        Task DeleteBuddy(int BuddyId);
    }
}
