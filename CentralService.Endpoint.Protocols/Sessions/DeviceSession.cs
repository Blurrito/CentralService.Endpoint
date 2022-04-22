using CentralService.Endpoint.DTO.Authentication;
using CentralService.Endpoint.DTO.User;
using CentralService.Endpoint.Protocols.Structs.Gpcm;
using CentralService.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Sessions
{
    public class DeviceSession
    {
        public int SessionKey { get; set; }
        public int DeviceProfileId { get; }
        public int GameProfileId { get; }
        public string UniqueNickname { get; }
        public string DeviceAddress { get; }
        public string GameCode { get; }
        public string RegionalGameCode { get; }
        public int ConsoleType { get; }

        public int Status { get; private set; }
        public string StatusString { get; private set; }
        public string LocationString { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }
        public int Qm { get; private set; }

        private List<Buddy> _Buddies = new List<Buddy>();
        private readonly object _BuddiesLock = new object();

        private List<BuddyForwardMessage> _PendingMessages = new List<BuddyForwardMessage>();
        private readonly object _PendingMessagesLock = new object();

        public DeviceSession(ChallengeProof Proof, List<Buddy> Buddies)
        {
            DeviceProfileId = Proof.DeviceProfileId;
            GameProfileId = Proof.GameProfileId;
            UniqueNickname = Proof.UniqueNickname;
            DeviceAddress = Proof.Address;
            _Buddies = Buddies;
            GameCode = Proof.GameCode;
            RegionalGameCode = Proof.RegionalGameCode;

            SessionKey = 0;
            Status = 0;
            StatusString = string.Empty;
            LocationString = string.Empty;
            Address = "0";
            Port = 0;
            Qm = 0;
        }

        public void Update(StatusUpdate Update)
        {
            Status = Update.status;
            StatusString = Update.statstring;
            LocationString = Update.locstring;
        }

        public void Update(string PublicIp) => Address = PublicIp;

        public void SendMessage(BuddyForwardMessage Message)
        {
            lock (_BuddiesLock)
            {
                Buddy Sender = _Buddies.FirstOrDefault(x => x.RecipientId == Message.f);
                if (Sender.Status == 2)
                    lock (_PendingMessagesLock)
                        _PendingMessages.Add(Message);
            }
        }

        public List<BuddyForwardMessage> GetPendingMessages()
        {
            List<BuddyForwardMessage> ReturnList = new List<BuddyForwardMessage>();
            lock (_PendingMessagesLock)
                if (_PendingMessages.Count > 0)
                {
                    ReturnList = _PendingMessages;
                    _PendingMessages = new List<BuddyForwardMessage>();
                }
            return ReturnList;
        }

        public List<Buddy> GetAcceptedRequests()
        {
            List<Buddy> FoundRequests;
            lock (_BuddiesLock)
                FoundRequests = _Buddies.Where(x => x.Status == 1).ToList();
            return FoundRequests;
        }

        public List<Buddy> GetAuthorizedRequests()
        {
            List<Buddy> FoundRequests;
            lock (_BuddiesLock)
                FoundRequests = _Buddies.Where(x => x.Status == 2).ToList();
            return FoundRequests;
        }

        public Buddy GetBuddy(int RecipientId)
        {
            Buddy FoundBuddy;
            lock (_BuddiesLock)
                FoundBuddy = _Buddies.FirstOrDefault(x => x.RecipientId == RecipientId);
            return FoundBuddy;
        }

        public void AddBuddy(Buddy NewBuddy)
        {
            lock (_BuddiesLock)
                _Buddies.Add(NewBuddy);
        }

        public void UpdateBuddy(Buddy UpdatedBuddy)
        {
            lock (_BuddiesLock)
            {
                Buddy ExistingBuddy = _Buddies.FirstOrDefault(x => x.BuddyId == UpdatedBuddy.BuddyId);
                _Buddies.Remove(ExistingBuddy);
                _Buddies.Add(UpdatedBuddy);
            }
        }

        public void DeleteBuddy(int RecipientId)
        {
            lock (_BuddiesLock)
            {
                Buddy ExistingBuddy = _Buddies.FirstOrDefault(x => x.BuddyId == RecipientId);
                _Buddies.Remove(ExistingBuddy);
            }
        }

        public string GetFriendCode()
        {
            byte[] Buffer = BitConverter.GetBytes(GameProfileId);
            Buffer.Concat(Encoding.UTF8.GetBytes(RegionalGameCode));

            byte Checksum = 0;
            switch (ConsoleType)
            {
                case 0:
                    Checksum = (byte)(Utilities.ComputeCRC8(Buffer) & 0x7F);
                    break;
                case 1:
                    Checksum = (byte)((Utilities.ComputeMD5(Buffer)[0] >> 1) & 0x7F);
                    break;
                default:
                    break;
            }
            long NumericFriendCode = (Checksum << 32) | GameProfileId;
            return NumericFriendCode.ToString().PadLeft(12, '0');
        }

        public override string ToString() => $"|s|{ Status }|ss|{ StatusString }|ls|{ LocationString }|ip|{ Address }|p|{ Port }|qm|{ Qm }";
    }
}
