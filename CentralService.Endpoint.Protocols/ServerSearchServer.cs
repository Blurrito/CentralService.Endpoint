using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.Matchmaking;
using CentralService.Endpoint.Protocols.Structs.Matchmaking;
using CentralService.Endpoint.Protocols.Sessions;
using CentralService.Endpoint.Factories.Clients;
using CentralService.Endpoint.Interfaces.Clients;
using CentralService.Endpoint.Interfaces.Protocols;
using CentralService.Endpoint.Protocols.Managers;
using CentralService.Utility;
using CentralService.Utility.Streams;
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
    public class ServerSearchServer : ITcpServer
    {
        public string Address => ((IPEndPoint)_Client.Client.RemoteEndPoint).Address.ToString();
        public int Port => ((IPEndPoint)_Client.Client.RemoteEndPoint).Port;

        private bool _Connected = true;
        private TcpClient _Client;
        private NetworkStream _Stream;

        private string _GameKey = string.Empty;
        private GetServerResponse _FoundServers;
        private MatchmakingSession _ClientSession;

        public async Task HandleClient(TcpClient Client, Stream Stream)
        {
            _Client = Client;
            _Stream = (NetworkStream)Stream;

            _ClientSession = MatchmakingSessionManager.GetSession(Address);
            if (_ClientSession == null)
                _Connected = false;

            short RequestLength = 0;
            byte[] Buffer = null;
            while (_Connected)
            {
                if (_Client.Available > 0)
                {
                    if (Buffer == null)
                    {
                        Buffer = new byte[_Client.Available];
                        await _Stream.ReadAsync(Buffer, 0, _Client.Available);
                        using (BigEndianReader Reader = new BigEndianReader(new MemoryStream(Buffer)))
                            RequestLength = Reader.ReadInt16();
                    }
                    else
                    {
                        byte[] AdditionalBuffer = new byte[_Client.Available];
                        await _Stream.ReadAsync(AdditionalBuffer, 0, _Client.Available);
                        Buffer = Buffer.Concat(AdditionalBuffer).ToArray();
                    }
                    if (Buffer.Length >= RequestLength)
                    {
                        await ProcessRequest(new ServerSearchRequest(Buffer));
                        Buffer = null;
                    }
                }
                else
                {
                    if (_Client.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] Tmp = new byte[1];
                        if (_Client.Client.Receive(Tmp, SocketFlags.Peek) == 0)
                            _Connected = false;
                    }
                    else
                        await Task.Delay(100);
                }
            }
        }

        private async Task ProcessRequest(ServerSearchRequest Request)
        {
            try
            {
                switch (Request.Type)
                {
                    case 0:
                        await ProcessSearchRequest(new GetServerRequest(Request.Data));
                        break;
                    case 2:
                        ProcessStartConnectionRequest(new StartConnectionRequest(Request));
                        break;
                    default:
                        throw new NotImplementedException($"Unknown request type: { Request.Type }");
                }
            }
            catch
            {

            }
        }

        private async Task ProcessSearchRequest(GetServerRequest Request)
        {
            _GameKey = GameKeyCollection.GetGameKey(Request.GameName);
            if (_GameKey == null)
                throw new KeyNotFoundException("No key could be found for the provided game name.");

            string Address = _ClientSession.GetHeartbeatPropertyValue("publicip");
            if (Address == null)
                throw new KeyNotFoundException("Public address could not be found among the heartbeat properties of the client.");

            using (IMatchmakingClient Client = MatchmakingClientFactory.GetClient())
                _FoundServers = GetResponseObject<GetServerResponse>(await Client.GetMultiplayerServers(Request));
            await Send(0, _FoundServers.ToByteArray(int.Parse(Address), Convert.ToUInt16(Port)), Request.ValidationString);
        }

        private void ProcessStartConnectionRequest(StartConnectionRequest Request)
        {
            GetServerResponseServer ChosenServer = _FoundServers.FoundServers.FirstOrDefault(x => x.PublicAddress == Request.Address && x.PublicPort == Request.Port);
            if (ChosenServer.PublicAddress > 0)
            {
                byte[] CompleteMessage = new byte[Request.Message.Length + 11];
                using (BinaryWriter Writer = new BinaryWriter(new MemoryStream(CompleteMessage)))
                {
                    Writer.Write(new byte[] { 0x0FE, 0xFD, 0x06 });
                    Writer.Write(ChosenServer.SessionId);
                    Writer.Write(DateTime.Now.Millisecond);
                    Writer.Write(Request.Message);
                }
                MatchmakingMessageManager.AddMessage(new MatchmakingMessage(IPAddress.Parse(ChosenServer.ActualAddress), ChosenServer.PublicPort, CompleteMessage));
            }
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

        private async Task Send(byte Type, byte[] Response, string ValidationString)
        {
            byte[] EncryptedResponse = EncTypeX.Encrypt(_GameKey, ValidationString, Response);
            await _Stream.WriteAsync(EncryptedResponse, 0, EncryptedResponse.Length);
        }

        public void Dispose()
        {
            _Stream.Close();
            _Stream.Dispose();
            _Client.Close();
        }
    }
}
