using CentralService.Endpoint.DTO.Authentication;
using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.Matchmaking;
using CentralService.Endpoint.Protocols.Structs.Matchmaking;
using CentralService.Endpoint.Protocols.Sessions;
using CentralService.Endpoint.Factories.Clients;
using CentralService.Endpoint.Interfaces.Clients;
using CentralService.Endpoint.Interfaces.Protocols;
using CentralService.Endpoint.Protocols.Managers;
using CentralService.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols
{
    public class MatchmakingServer : IUdpServer
    {
        private IPAddress _Address;
        private short _Port;

        public async Task<byte[]> HandleClient(IPAddress Address, short Port, byte[] Data)
        {
            _Address = Address;
            _Port = Port;

            try
            {
                MatchmakingRequest Request = new MatchmakingRequest(Data);
                switch (Request.RequestType)
                {
                    case 1:
                        return await ProcessLoginChallenge(Request);
                    case 3:
                        return await ProcessHeartbeat(Request);
                    case 7:
                        //Client Acknowledge
                        return null;
                    case 8:
                        ProcessKeepAlive(Request);
                        return null;
                    case 9:
                        return new byte[] { 0xFE, 0xFD, 0x09, 0x00, 0x00, 0x00, 0x00 };
                    default:
                        throw new NotImplementedException($"Matchmaking server - Unimplemented command { Request.RequestType }.");
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"Matchmaking server - Exception => { Ex.Message }");
                return null;
            }
        }

        public List<MatchmakingMessage> GetPendingMessages()
        {
            if (MatchmakingMessageManager.HasPendingMessages())
                return MatchmakingMessageManager.GetMessages();
            return new List<MatchmakingMessage>();
        }

        public void Dispose() { }

        private async Task<byte[]> ProcessLoginChallenge(MatchmakingRequest Request)
        {
            bool Success = false;
            string ChallengeResult = Encoding.UTF8.GetString(Request.Data, 0, 28);
            using (IAuthenticationClient Client = AuthenticationClientFactory.GetClient())
                Success = GetResponseObject<bool>(await Client.ValidateMatchmakingChallengeResult(new MatchmakingChallengeResult(Request.ClientId, ChallengeResult)));
            if (Success)
            {
                MatchmakingSession Session = MatchmakingSessionManager.GetSession(Request.ClientId);
                if (Session != null)
                    Session.IsAuthenticated = true;

                byte[] Response = new byte[18];
                using (BinaryWriter Writer = new BinaryWriter(new MemoryStream(Response)))
                {
                    Writer.Write(new byte[] { 0xFE, 0xFD, 0x0A });
                    Writer.Write(Request.ClientId);
                }
                return Response;
            }
            return null;
        }

        private void ProcessKeepAlive(MatchmakingRequest Request)
        {
            MatchmakingSession Session = MatchmakingSessionManager.GetSession(Request.ClientId);
            if (Session != null)
                Session.ValidUntil = DateTime.Now.AddMinutes(3);
        }

        private async Task<byte[]> ProcessHeartbeat(MatchmakingRequest Request)
        {
            string ValueString = Encoding.UTF8.GetString(Request.Data);
            string[] SplitRequest = ValueString.Split('\0').Where(x => x != string.Empty).ToArray();
            if (SplitRequest.Length % 2 != 0)
                throw new ArgumentException("Provided request data incomplete or corrupted", nameof(Request.Data));

            List<KeyValuePair<string, string>> Values = new List<KeyValuePair<string, string>>();
            for (int i = 0; i < SplitRequest.Length; i += 2)
                Values.Add(new KeyValuePair<string, string>(SplitRequest[i], SplitRequest[i + 1]));

            string ProfileId = Values.FirstOrDefault(x => x.Key == "dwc_pid").Value;
            string PublicAddress = Values.FirstOrDefault(x => x.Key == "publicip").Value;
            string PublicPort = Values.FirstOrDefault(x => x.Key == "publicport").Value;
            string LocalPort = Values.FirstOrDefault(x => x.Key == "localport").Value;
            string State = Values.FirstOrDefault(x => x.Key == "statechanged").Value;
            string GameName = Values.FirstOrDefault(x => x.Key == "gamename").Value;

            MatchmakingSession Session = GetSession(Request.ClientId);
            if (PublicAddress == null)
            {
                PublicAddress = GetPublicAddress();
                Values.Add(new KeyValuePair<string, string>("publicip", PublicAddress));
            }
            else if (PublicAddress == "0")
            {
                PublicAddress = GetPublicAddress();
                Values.RemoveAll(x => x.Key == "publicip");
                Values.Add(new KeyValuePair<string, string>("publicip", PublicAddress));
            }

            PublicPort = GetPublicPort(PublicPort, LocalPort);
            Values.RemoveAll(x => x.Key == "publicport");
            Values.Add(new KeyValuePair<string, string>("publicport", PublicPort));
            UpdateDeviceSession(ProfileId, PublicAddress);

            if (Session.DeviceType == -1)
                Session.DeviceType = GetConsoleType(GameName, ProfileId);
            if (State != null && ((!Session.ChallengeSent && !Session.IsAuthenticated) || (Session.ChallengeSent && Session.IsAuthenticated)))
                await UpdateMatchmakingServer(Session.SessionId, Session.DeviceType, int.Parse(State), Values);
            Session.UpdateHeartbeatProperties(Values);

            if (!Session.ChallengeSent)
                return await GenerateChallenge(Session, GameName, PublicAddress, PublicPort);
            return null;
        }

        private MatchmakingSession GetSession(int SessionId)
        {
            MatchmakingSession Session = MatchmakingSessionManager.GetSession(SessionId);
            if (Session == null)
            {
                Session = new MatchmakingSession(SessionId, _Address, _Port);
                MatchmakingSessionManager.AddSession(Session);
            }
            return Session;
        }

        private string GetPublicAddress()
        {
            string NewPublicAddress = string.Empty;
            byte[] AddressBytes = _Address.GetAddressBytes();
            foreach (byte AddressByte in AddressBytes)
            {
                if (AddressByte < 26)
                    NewPublicAddress += '0';
                if (AddressByte < 10)
                    NewPublicAddress += '0';
                NewPublicAddress += Convert.ToString(AddressByte);
            }
            return NewPublicAddress;
        }

        private string GetPublicPort(string PublicPort, string LocalPort)
        {
            if (PublicPort != null && LocalPort != null)
                if (PublicPort == LocalPort)
                    return PublicPort;
            return _Port.ToString();
        }

        private int GetConsoleType(string GameName, string GameProfileId)
        {
            if (GameName != null)
            {
                if (GameName.Contains("ds"))
                    return 0;
                else if (GameName.Contains("wii"))
                    return 1;
            }
            return -1;
        }

        private void UpdateDeviceSession(string GameProfileId, string PublicAddress)
        {
            int ConvertedProfileId = 0;
            if (int.TryParse(GameProfileId, out ConvertedProfileId))
            {
                DeviceSession ExistingSession = DeviceSessionManager.GetDeviceSession(ConvertedProfileId);
                if (ExistingSession != null)
                    ExistingSession.Update(PublicAddress);
            }
        }

        private async Task UpdateMatchmakingServer(int SessionId, int Console, int State, List<KeyValuePair<string, string>> Properties)
        {
            ServerUpdate Update = new ServerUpdate(SessionId, Console, State, _Address.ToString(), Properties);
            using (IMatchmakingClient Client = MatchmakingClientFactory.GetClient())
                await Client.ManageMultiplayerServer(Update);
        }

        private async Task<byte[]> GenerateChallenge(MatchmakingSession Session, string GameName, string PublicAddress, string PublicPort)
        {
            string ChallengeString;
            if (GameName == null || PublicAddress == null || PublicPort == null)
                return null;

            using (IAuthenticationClient Client = AuthenticationClientFactory.GetClient())
                ChallengeString = GetResponseObject<string>(await Client.GetMatchmakingChallenge(Session.SessionId, GameName,
                    Utilities.AddressToHexadecimalString(_Address), Utilities.PortToHexadecimalString(_Port)));
            MatchmakingChallenge Challenge = new MatchmakingChallenge(Session.SessionId, ChallengeString, _Address, _Port);
            Session.ChallengeSent = true;
            return Challenge.ToByteArray();
        }

        private TType GetResponseObject<TType>(ApiResponse? Response)
        {
            if (Response != null)
                if (Response.Value.Content != null)
                    if (Response.Value.Success)
                        return (TType)Response.Value.Content;
                    else
                        throw new ArgumentException("The obtained response contains an error response.", nameof(Response.Value.Content))
                        { Data = { { "Error", (ApiError)Response.Value.Content } } };
                else
                    throw new ArgumentException("The obtained response contains no body.");
            else
                throw new ArgumentException("The obtained response is empty.");
        }
    }
}
