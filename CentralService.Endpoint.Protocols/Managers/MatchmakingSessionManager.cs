using CentralService.Endpoint.Protocols.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols
{
    public static class MatchmakingSessionManager
    {
        private static List<MatchmakingSession> _Sessions = new List<MatchmakingSession>();
        private static readonly object _SessionsLock = new object();

        public static MatchmakingSession GetSession(int SessionId)
        {
            MatchmakingSession Session;
            lock (_SessionsLock)
                Session = _Sessions.FirstOrDefault(x => x.SessionId == SessionId);
            return Session;
        }

        public static MatchmakingSession GetSession(string Address)
        {
            MatchmakingSession Session;
            lock (_SessionsLock)
                Session = _Sessions.FirstOrDefault(x => x.PublicAddress.ToString() == Address);
            return Session;
        }

        public static void AddSession(MatchmakingSession Session)
        {
            lock (_SessionsLock)
            {
                MatchmakingSession ExistingSession = _Sessions.FirstOrDefault(x => x.SessionId == Session.SessionId);
                if (ExistingSession == null)
                    _Sessions.Add(Session);
            }
        }

        public static void RemoveSession(int SessionId)
        {
            lock (_SessionsLock)
                _Sessions.RemoveAll(x => x.SessionId == SessionId);
        }
    }
}
