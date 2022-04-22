using CentralService.Endpoint.DTO.User;
using CentralService.Endpoint.Protocols.Structs.Gpcm;
using CentralService.Endpoint.Protocols.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols
{
    public static class DeviceSessionManager
    {
        private static List<DeviceSession> _DeviceSessions = new List<DeviceSession>();
        private static readonly object _DeviceSessionLock = new object();

        public static DeviceSession GetDeviceSession(int GameProfileId)
        {
            DeviceSession FoundSession;
            lock (_DeviceSessionLock)
                FoundSession = _DeviceSessions.FirstOrDefault(x => x.GameProfileId == GameProfileId);
            return FoundSession;
        }

        public static int AddDeviceSession(DeviceSession DeviceSession)
        {
            lock (_DeviceSessionLock)
            {
                if (_DeviceSessions.FirstOrDefault(x => x.DeviceProfileId == DeviceSession.DeviceProfileId && x.GameProfileId == DeviceSession.GameProfileId) != null)
                    return 0;
                Random Random = new Random();
                do
                {
                    DeviceSession.SessionKey = Random.Next(0, int.MaxValue - 1);
                }
                while (_DeviceSessions.FirstOrDefault(x => x.SessionKey == DeviceSession.SessionKey) != null);
                _DeviceSessions.Add(DeviceSession);
            }
            return DeviceSession.SessionKey;
        }

        public static void RemoveDeviceSession(DeviceSession Session)
        {
            lock (_DeviceSessionLock)
                _DeviceSessions.Remove(Session);
        }

        public static void SendMessage(List<Buddy> Recipients, int MessageType, string Message, string Date = null)
        {
            lock (_DeviceSessionLock)
            {
                foreach (Buddy Recipient in Recipients)
                {
                    DeviceSession Session = _DeviceSessions.FirstOrDefault(x => x.GameProfileId == Recipient.RecipientId);
                    if (Session != null)
                        Session.SendMessage(new BuddyForwardMessage(MessageType, Recipient.SenderId, null, Message));
                }
            }
        }

        public static void SendMessage(int SenderId, BuddyMessage Message)
        {
            lock (_DeviceSessionLock)
            {
                DeviceSession Session = _DeviceSessions.FirstOrDefault(x => x.GameProfileId == Message.t);
                if (Session != null)
                    Session.SendMessage(new BuddyForwardMessage(SenderId, Message));
            }
        }

        public static void SendMessage(Buddy Recipient, int MessageType, string Message, string Date = null)
        {
            lock (_DeviceSessionLock)
            {
                DeviceSession Session = _DeviceSessions.FirstOrDefault(x => x.GameProfileId == Recipient.RecipientId);
                if (Session != null)
                    Session.SendMessage(new BuddyForwardMessage(MessageType, Recipient.SenderId, Date, Message));
            }
        }

        public static string GetDeviceStatus(int GameProfileId)
        {
            string Status = null;
            lock (_DeviceSessionLock)
            {
                DeviceSession Session = _DeviceSessions.FirstOrDefault(x => x.GameProfileId == GameProfileId);
                if (Session != null)
                    Status = Session.ToString();
            }
            return Status;
        }

        public static void Dispose() { }
    }
}
