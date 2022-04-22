using CentralService.Endpoint.DTO.Authentication;
using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.User;
using CentralService.Endpoint.Factories.Clients;
using CentralService.Endpoint.Interfaces.Clients;
using CentralService.Endpoint.Interfaces.Protocols;
using CentralService.Endpoint.Protocols.Protocols;
using CentralService.Endpoint.Protocols.Sessions;
using CentralService.Endpoint.Protocols.Structs.Gpcm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols
{
    public class ProfileServer : ITcpServer
    {
        public string Address => ((IPEndPoint)_Client.Client.RemoteEndPoint).Address.ToString();

        private bool _Connected = true;
        private DeviceSession _Session = null;
        private int _CurrentMessageId = 1;
        private TcpClient _Client;
        private NetworkStream _Stream;

        private int _MessageTimer = 0;
        private bool _MessageReceived = false;

        public async Task HandleClient(TcpClient Client, Stream Stream)
        {
            _Client = Client;
            _Stream = (NetworkStream)Stream;

            await SendLoginChallenge();
            while (_Connected)
            {
                if (_Client.Available > 0)
                {
                    _MessageReceived = true;
                    byte[] Buffer = new byte[_Client.Available];
                    await _Stream.ReadAsync(Buffer, 0, _Client.Available);
                    await ProcessRequest(Encoding.UTF8.GetString(Buffer));
                }
                else
                {
                    await AdvanceMessageTimer();
                    await Task.Delay(100);
                }
            }
            if (_Session != null)
                ProcessLogoutRequest();
        }

        private async Task AdvanceMessageTimer()
        {
            try
            {
                _MessageTimer += 100;
                if (_MessageTimer % 1000m == 0 && _Session != null)
                {
                    List<BuddyForwardMessage> PendingMessages = _Session.GetPendingMessages();
                    foreach (BuddyForwardMessage Message in PendingMessages)
                        await Send(Message);
                }
                if (_MessageTimer % 180000m == 0)
                {
                    if (!_MessageReceived)
                        await Send(new KeepAlive());
                    else
                        _MessageReceived = false;
                    _MessageTimer = 0;
                }
            }
            catch
            {
                _Connected = false;
            }
        }

        private async Task ProcessRequest(string Request)
        {
            try
            {
                List<List<KeyValuePair<string, string>>> Requests = GamespyTcpProtocol.GetRequests(Request);
                foreach (List<KeyValuePair<string, string>> Properties in Requests)
                {
                    switch (Properties[0].Key)
                    {
                        case "login":
                            await ProcessLoginChallengeResult(GamespyTcpProtocol.Deserialize<LoginChallengeResult>(Properties));
                            break;
                        case "getprofile":
                            await ProcessGetProfileRequest(GamespyTcpProtocol.Deserialize<GetProfile>(Properties));
                            break;
                        case "updatepro":
                            await ProcessUpdateProfileRequest(GamespyTcpProtocol.Deserialize<UpdateProfile>(Properties));
                            break;
                        case "status":
                            ProcessStatusUpdate(GamespyTcpProtocol.Deserialize<StatusUpdate>(Properties));
                            break;
                        case "bm":
                            ProcessBuddyMessage(GamespyTcpProtocol.Deserialize<BuddyMessage>(Properties));
                            break;
                        case "addbuddy":
                            await ProcessAddBuddyRequest(GamespyTcpProtocol.Deserialize<AddBuddy>(Properties));
                            break;
                        case "authadd":
                            await ProcessAuthenticateBuddyRequest(GamespyTcpProtocol.Deserialize<AuthenticateBuddy>(Properties));
                            break;
                        case "delbuddy":
                            await ProcessDeleteBuddyRequest(GamespyTcpProtocol.Deserialize<DeleteBuddy>(Properties));
                            break;
                        case "logout":
                            if (_Session != null)
                                ProcessLogoutRequest();
                            break;
                        default:
                            throw new NotImplementedException($"Unimplemented command { Properties[0].Key }.");
                    }
                }
            }
            catch (Exception Ex)
            {
                if (Ex.Data.Contains("Error"))
                {
                    ApiError Error = (ApiError)Ex.Data["Error"];
                    if (Error.IsFatal)
                        _Connected = false;
                    await Send(new Error(Error, _CurrentMessageId));
                }
                else
                    _Connected = false;
            }
        }

        private async Task SendLoginChallenge()
        {
            string ChallengeString = string.Empty;
            using (IAuthenticationClient Client = AuthenticationClientFactory.GetClient())
                ChallengeString = GetResponseObject<string>(await Client.GetChallenge(Address));
            await Send(new LoginChallenge(ChallengeString));
        }

        private async Task ProcessLoginChallengeResult(LoginChallengeResult Result)
        {
            ChallengeProof Proof;
            ChallengeResult ChallengeResult = new ChallengeResult(Result.authtoken, Result.challenge, Result.response);
            using (IAuthenticationClient Client = AuthenticationClientFactory.GetClient())
                Proof = GetResponseObject<ChallengeProof>(await Client.ValidateChallengeResponse(Address, ChallengeResult));

            List<Buddy> Buddies;
            using (IBuddyClient Client = BuddyClientFactory.GetClient())
                Buddies = GetResponseObject<List<Buddy>>(await Client.GetBuddyList(Proof.GameProfileId));

            _Session = new DeviceSession(Proof, Buddies);
            DeviceSessionManager.AddDeviceSession(_Session);
            await Send(new LoggedIn(Proof, _Session.SessionKey, _CurrentMessageId));
            await SendPendingMessages();
            await SendBuddyStatusMessages();
        }

        private async Task<List<Buddy>> GetPendingMessages()
        {
            List<Buddy> IncomingRequests = _Session.GetAcceptedRequests();
            using (IBuddyClient Client = BuddyClientFactory.GetClient())
            {
                List<Buddy> NewRequests = GetResponseObject<List<Buddy>>(await Client.GetIncomingRequests(_Session.GameProfileId));
                foreach (Buddy Request in NewRequests)
                    IncomingRequests.Add(Request);
            }
            return IncomingRequests;
        }

        private async Task SendPendingMessages()
        {
            List<Buddy> IncomingRequests = await GetPendingMessages();
            using (IBuddyClient Client = BuddyClientFactory.GetClient())
                foreach (Buddy Buddy in IncomingRequests)
                    if (Buddy.Status == 1)
                    {
                        await Send(new BuddyForwardMessage(1, Buddy.SenderId, Buddy.Date.ToString(), "I have authorized your request to add me to your list."));
                        Buddy UpdatedBuddy = new Buddy(Buddy, 2);
                        _Session.UpdateBuddy(UpdatedBuddy);
                        await Client.UpdateBuddy(UpdatedBuddy);
                    }
                    else
                        await Send(new BuddyForwardMessage(2, Buddy.SenderId, Buddy.Date.ToString(), "\r\n\r\n|signed|8f1f2eaa285bce91054a3768b3dbf141"));
        }

        private async Task SendBuddyStatusMessages()
        {
            List<Buddy> Buddies = _Session.GetAuthorizedRequests();
            foreach (Buddy Buddy in Buddies)
            {
                string Status = DeviceSessionManager.GetDeviceStatus(Buddy.RecipientId);
                if (Status != null)
                    await Send(new BuddyForwardMessage(100, Buddy.RecipientId, null, Status));
            }
        }

        private void ProcessBuddyMessage(BuddyMessage Message) => DeviceSessionManager.SendMessage(_Session.GameProfileId, Message);

        private async Task ProcessGetProfileRequest(GetProfile Request)
        {
            if (Request.sesskey != _Session.SessionKey || _Session.SessionKey == 0)
                throw new ArgumentException("Invalid session key.");
            _CurrentMessageId = Request.id;

            GameProfile Profile;
            using (IGameProfileClient Client = GameProfileClientFactory.GetClient())
                Profile = GetResponseObject<GameProfile>(await Client.GetGameProfile(Request.profileid));
            await Send(new ProfileInfo(Profile, _CurrentMessageId));
        }

        private async Task ProcessUpdateProfileRequest(UpdateProfile Request)
        {
            if (Request.sesskey != _Session.SessionKey || _Session.SessionKey == 0)
                throw new ArgumentException("Invalid session key.");

            using (IGameProfileClient Client = GameProfileClientFactory.GetClient())
                await Client.UpdateGameProfile(Request.sesskey, new GameProfile(_Session.GameProfileId, Request.nick, Request.firstname, Request.lastname, Request.zipcode, Request.aim, Request.lon, Request.lat, Request.loc));
        }

        private void ProcessStatusUpdate(StatusUpdate Update)
        {
            if (_Session.SessionKey == 0)
                throw new ArgumentException("Invalid session key.");

            _Session.Update(Update);
            List<Buddy> RegisteredBuddies = _Session.GetAuthorizedRequests();
            DeviceSessionManager.SendMessage(RegisteredBuddies, 100, _Session.ToString());
        }

        private void ProcessLogoutRequest()
        {
            if (_Session.SessionKey > 0)
            {
                DeviceSessionManager.RemoveDeviceSession(_Session);
                List<Buddy> RegisteredBuddies = _Session.GetAuthorizedRequests();
                DeviceSessionManager.SendMessage(RegisteredBuddies, 100, "|s|0|ss|Offline");
                _Connected = false;
            }
        }

        private async Task ProcessAddBuddyRequest(AddBuddy Request)
        {
            if (Request.sesskey != _Session.SessionKey || _Session.SessionKey == 0)
                throw new ArgumentException("Invalid session key.");
            if (Request.newprofileid != _Session.GameProfileId)
            {
                Buddy NewBuddy = new Buddy(_Session.GameProfileId, Request.newprofileid);
                int NewBuddyId = 0;
                using (IBuddyClient Client = BuddyClientFactory.GetClient())
                    NewBuddyId = GetResponseObject<int>(await Client.AddBuddy(_Session.GameCode, NewBuddy));

                if (NewBuddyId > 0)
                {
                    _Session.AddBuddy(NewBuddy);
                    DeviceSessionManager.SendMessage(NewBuddy, 2, string.Empty);
                }
            }
        }

        private async Task ProcessAuthenticateBuddyRequest(AuthenticateBuddy Request)
        {
            if (Request.sesskey != _Session.SessionKey || _Session.SessionKey == 0)
                throw new ArgumentException("Invalid session key.");

            Buddy UpdatedBuddy = new Buddy(_Session.GameProfileId, Request.fromprofileid, 1);
            using (IBuddyClient Client = BuddyClientFactory.GetClient())
                await Client.UpdateBuddy(UpdatedBuddy);
            DeviceSessionManager.SendMessage(UpdatedBuddy, 1, "I have authorized your request to add me to your list.");
        }

        private async Task ProcessDeleteBuddyRequest(DeleteBuddy Request)
        {
            if (Request.sesskey != _Session.SessionKey || _Session.SessionKey == 0)
                throw new ArgumentException("Invalid session key.");

            Buddy ExistingBuddy = _Session.GetBuddy(Request.delprofileid);
            if (ExistingBuddy.BuddyId > 0)
                using (IBuddyClient Client = BuddyClientFactory.GetClient())
                    await Client.DeleteBuddy(ExistingBuddy.BuddyId);
            _Session.DeleteBuddy(ExistingBuddy.RecipientId);
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

        private async Task Send<TType>(TType Response) where TType : struct
        {
            string SerializedResponse = GamespyTcpProtocol.Serialize(Response);
            byte[] ResponseByteArray = Encoding.UTF8.GetBytes(SerializedResponse);
            await _Stream.WriteAsync(ResponseByteArray, 0, ResponseByteArray.Length);
        }

        public void Dispose()
        {
            _Stream.Close();
            _Stream.Dispose();
            _Client.Close();
        }
    }
}
