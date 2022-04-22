using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.DTO.Matchmaking;

namespace CentralService.Endpoint.Protocols.Managers
{
    public static class MatchmakingMessageManager
    {
        private static List<MatchmakingMessage> _Messages = new List<MatchmakingMessage>();
        private static readonly object _MessagesLock = new object();

        public static bool HasPendingMessages()
        {
            bool HasPendingMessages = false;
            lock (_MessagesLock)
                HasPendingMessages = _Messages.Count > 0;
            return HasPendingMessages;
        }

        public static List<MatchmakingMessage> GetMessages()
        {
            List<MatchmakingMessage> ReturnList;
            lock (_MessagesLock)
            {
                ReturnList = _Messages;
                _Messages = new List<MatchmakingMessage>();
            }
            return ReturnList;
        }

        public static void AddMessage(MatchmakingMessage Message)
        {
            lock (_MessagesLock)
                _Messages.Add(Message);
        }
    }
}
