using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.User;
using CentralService.Endpoint.Protocols.Structs.Gpcm;
using CentralService.Endpoint.Protocols.Sessions;
using CentralService.Endpoint.Protocols.Protocols;
using CentralService.Endpoint.Factories.Clients;
using CentralService.Endpoint.Interfaces.Clients;
using CentralService.Endpoint.Interfaces.Protocols;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols
{
    public class BuddySearchServer : ITcpServer
    {
        public string Address => ((IPEndPoint)_Client.Client.RemoteEndPoint).Address.ToString();

        private bool _Connected = true;
        private TcpClient _Client;
        private NetworkStream _Stream;

        public async Task HandleClient(TcpClient Client, Stream Stream)
        {
            _Client = Client;
            _Stream = (NetworkStream)Stream;

            while (_Connected)
            {
                if (_Client.Available > 0)
                {
                    byte[] Buffer = new byte[_Client.Available];
                    await _Stream.ReadAsync(Buffer, 0, _Client.Available);
                    await ProcessRequest(Encoding.UTF8.GetString(Buffer));
                }
                else
                    await Task.Delay(100);
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
                        case "search":
                            await ProcessBuddySearchRequest(GamespyTcpProtocol.Deserialize<BuddySearchRequest>(Properties));
                            _Connected = false;
                            break;
                        default:
                            throw new NotImplementedException($"Command { Properties[0].Key } has not been implemented.");
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
                    await Send(new Error(Error, 1));
                }
                else
                    _Connected = false;
            }
        }

        private async Task ProcessBuddySearchRequest(BuddySearchRequest Request)
        {
            DeviceSession Session = DeviceSessionManager.GetDeviceSession(Request.profileid);
            if (Session == null)
                throw new ArgumentException($"No session could be found for user with profile ID { Request.profileid }.", nameof(Request.profileid));

            if (Session.SessionKey != Request.sesskey)
                throw new ArgumentException("Invalid session key.", nameof(Request.sesskey));

            GameProfile Profile;
            using (IGameProfileClient Client = GameProfileClientFactory.GetClient())
                Profile = GetResponseObject<GameProfile>(await Client.GetBuddyProfile(Request.profileid, Request.lastname));

            if (Profile.GameProfileId > 0)
                await Send(new BuddySearchResponse(Profile));
            else
                await Send(new BuddySearchResponse());
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
