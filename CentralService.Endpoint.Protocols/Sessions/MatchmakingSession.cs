using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Sessions
{
    public class MatchmakingSession
    {
        public int SessionId { get; set; }
        public IPAddress PublicAddress { get; set; }
        public int PublicPort { get; set; }
        public bool ChallengeSent { get; set; }
        public bool IsAuthenticated { get; set; }
        public int DeviceType { get; set; }
        public DateTime ValidUntil { get; set; }

        private List<KeyValuePair<string, string>> _HeartbeatProperties = new List<KeyValuePair<string, string>>();
        private readonly object _HeartbeatPropertiesLock = new object();

        public MatchmakingSession(int SessionId, IPAddress PublicAddress, int PublicPort)
        {
            this.SessionId = SessionId;
            this.PublicAddress = PublicAddress;
            this.PublicPort = PublicPort;
            ChallengeSent = false;
            IsAuthenticated = false;
            DeviceType = -1;
            ValidUntil = DateTime.Now.AddMinutes(3);
        }

        public string GetHeartbeatPropertyValue(string PropertyName)
        {
            lock (_HeartbeatPropertiesLock)
                return _HeartbeatProperties.FirstOrDefault(x => x.Key == PropertyName).Value;
        }

        public void UpdateHeartbeatProperties(List<KeyValuePair<string, string>> Properties)
        {
            lock (_HeartbeatPropertiesLock)
            {
                KeyValuePair<string, string>[] Buffer = new KeyValuePair<string, string>[Properties.Count];
                Properties.CopyTo(Buffer);
                _HeartbeatProperties = Buffer.ToList();
            }
        }
    }
}
