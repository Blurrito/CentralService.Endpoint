using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.DTO.Authentication;
using CentralService.Endpoint.DTO.Common;
using CentralService.Endpoint.DTO.ContentDelivery;
using CentralService.Endpoint.DTO.User;
using CentralService.Endpoint.Protocols.Structs.Dls1;
using CentralService.Endpoint.Protocols.Structs.Nas;
using CentralService.Endpoint.Protocols.Protocols;
using CentralService.Endpoint.Protocols.Protocols.Http;
using CentralService.Endpoint.Factories.Clients;
using CentralService.Endpoint.Interfaces.Clients;
using CentralService.Endpoint.Interfaces.Protocols;

namespace CentralService.Endpoint.Protocols
{
    public class NasServer : ITcpServer
    {
        public string Address => ((IPEndPoint)_Client.Client.RemoteEndPoint).Address.ToString();
        public bool Connected => _Connected && _Client.Connected;

        private bool _Connected = true;
        private TcpClient _Client;
        private SslStream _Stream;

        private readonly List<KeyValuePair<string, string>> NasResponseHeaders = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("NODE", "wifiappe2"),
            new KeyValuePair<string, string>("Date", DateTime.UtcNow.ToString("R")),
            new KeyValuePair<string, string>("Connection", "close"),
            new KeyValuePair<string, string>("Server", "Nintendo Wii(http)")
        };

        private readonly List<KeyValuePair<string, string>> Dls1ResponseHeaders = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Date", DateTime.UtcNow.ToString("R")),
            new KeyValuePair<string, string>("X-DLS-Host", "http://127.0.0.1/"),
            new KeyValuePair<string, string>("Server", "Nintendo Wii(http)")
        };

        public NasServer() { }

        public void Dispose()
        {
            _Stream.Close();
            _Stream.Dispose();
            _Client.Close();
        }

        public async Task HandleClient(TcpClient Client, Stream Stream)
        {
            _Client = Client;
            _Stream = (SslStream)Stream;

            while (Connected)
            {
                if (_Client.Available > 0)
                {
                    byte[] Buffer = new byte[Client.Available];
                    Stream.Read(Buffer, 0, Buffer.Length);
                    HttpRequest Request = new HttpRequest(Buffer);
                    //_Connected = KeepAlive(Request);
                    await ProcessRequest(Request);
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

        private async Task Send(HttpResponse Response)
        {
            byte[] ResponseByteArray = Response.GetBytes();
            await _Stream.WriteAsync(ResponseByteArray, 0, ResponseByteArray.Length);
        }

        //private bool KeepAlive(HttpRequest Request)
        //{
        //    KeyValuePair<string, string> KeepConnectionAlive = Request.Header.FirstOrDefault(x => x.Key == "Connection");
        //    if (Request.HttpVersion == "1.0")
        //    {
        //        if (KeepConnectionAlive.Key != null)
        //            if (KeepConnectionAlive.Value == "keep-alive")
        //                return true;
        //        return false;
        //    }
        //    else
        //    {
        //        if (KeepConnectionAlive.Key != null)
        //            if (KeepConnectionAlive.Value == "close")
        //                return false;
        //        return true;
        //    }
        //}

        private async Task ProcessRequest(HttpRequest Request)
        {
            HttpResponse Response;
            try
            {
                string ConvertedBody = Encoding.UTF8.GetString(Request.Body);
                switch (Request.EndpointUrl)
                {
                    case "/ac":
                        AcRequest AccountRequest = NasProtocol.Deserialize<AcRequest>(ConvertedBody);
                        Response = await ProcessAcRequest(AccountRequest);
                        break;
                    case "/download":
                        DownloadRequest DownloadRequest = NasProtocol.Deserialize<DownloadRequest>(ConvertedBody);
                        Response = await ProcessDownloadRequest(DownloadRequest);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                await Send(Response);
            }
            catch (ArgumentException Ex)
            {
                if (Ex.Data.Contains("Error"))
                {
                    ApiError Error = (ApiError)Ex.Data["Error"];
                    if (Error.IsFatal)
                        _Connected = false;
                    Response = FormulateAcResponse(new AcErrorResponse(Error));
                    await Send(Response);
                }
            }
        }

        private async Task<HttpResponse> ProcessAcRequest(AcRequest Request)
        {
            switch (Request.action)
            {
                case "acctcreate":
                    return await ProcessAcAcctCreate(Request);
                case "login":
                    return await ProcessAcLogin(Request);
                case "SVCLOC":
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<HttpResponse> ProcessAcAcctCreate(AcRequest Request)
        {
            int DeviceProfileId = 0;
            DeviceProfile Profile = new DeviceProfile(Request.userid, Request.passwd, Request.macadr, Request.devname, null);
            using (IDeviceProfileClient Client = DeviceProfileClientFactory.GetClient())
                DeviceProfileId = GetResponseObject<int>(await Client.CreateDeviceProfile(Profile));
            return FormulateAcResponse(new AcAcctCreateResponse(DeviceProfileId));
        }

        private async Task<HttpResponse> ProcessAcLogin(AcRequest Request)
        {
            NasToken Token;
            DeviceProfile Profile = new DeviceProfile(Request.userid, Request.passwd, Request.macadr, Request.devname, new GameProfile(Request.userid, Request.gamecd, Request.gsbrcd));
            using (IDeviceProfileClient Client = DeviceProfileClientFactory.GetClient())
                Token = GetResponseObject<NasToken>(await Client.Login(Address, Profile));
            if (Request.gsbrcd != null)
                if (Request.gsbrcd != string.Empty)
                    using (IAuthenticationClient Client = AuthenticationClientFactory.GetClient())
                        await Client.GenerateChallenge(Token);
            return FormulateAcResponse(new AcLoginResponse(Token.Token, Token.Challenge));
        }

        private async Task<HttpResponse> ProcessDownloadRequest(DownloadRequest Request)
        {
            switch (Request.action)
            {
                case "count":
                    return await ProcessGetFileCountRequest(Request);
                case "list":
                    return await ProcessGetFileListRequest(Request);
                case "contents":
                    return await ProcessGetFileContentRequest(Request);
                default:
                    throw new NotImplementedException();
            }
        }

        private async Task<HttpResponse> ProcessGetFileCountRequest(DownloadRequest Request)
        {
            int FileCount = 0;
            List<string> Attributes = new List<string>() { Request.attr1, Request.attr2, Request.attr3 };
            Attributes.RemoveAll(x => x == null);
            using (IContentDeliveryClient Client = ContentDeliveryClientFactory.GetClient())
                FileCount = GetResponseObject<int>(await Client.GetFileCount(new GetFileCountRequest(Request.gamecd, Attributes)));
            return FormulateDls1Response(Encoding.UTF8.GetBytes(FileCount.ToString()), "text/plain");
        }

        private async Task<HttpResponse> ProcessGetFileListRequest(DownloadRequest Request)
        {
            string FileList = string.Empty;
            List<string> Attributes = new List<string>() { Request.attr1, Request.attr2, Request.attr3 };
            Attributes.RemoveAll(x => x == null);
            using (IContentDeliveryClient Client = ContentDeliveryClientFactory.GetClient())
                FileList = GetResponseObject<string>(await Client.GetFileCount(new GetFileCountRequest(Request.gamecd, Attributes)));
            return FormulateDls1Response(Encoding.UTF8.GetBytes(FileList), "text/plain");
        }

        private async Task<HttpResponse> ProcessGetFileContentRequest(DownloadRequest Request)
        {
            byte[] FileContent;
            List<string> Attributes = new List<string>() { Request.attr1, Request.attr2, Request.attr3 };
            Attributes.RemoveAll(x => x == null);
            using (IContentDeliveryClient Client = ContentDeliveryClientFactory.GetClient())
                FileContent = GetResponseObject<byte[]>(await Client.GetFileCount(new GetFileCountRequest(Request.gamecd, Attributes)));
            return FormulateDls1Response(FileContent, "application/x-dsdl");
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

        private HttpResponse FormulateAcResponse<TType>(TType Response) where TType : struct
        {
            HttpResponse ReturnObject = new HttpResponse(new Version(1, 1), HttpStatusCode.OK);
            ReturnObject.ProcessHeaders(NasResponseHeaders);
            string SerializedResponseContent = NasProtocol.Serialize(Response);
            ReturnObject.ProcessBody(Encoding.UTF8.GetBytes(SerializedResponseContent), "text/plain");
            return ReturnObject;
        }

        private HttpResponse FormulateDls1Response(byte[] Response, string ContentType)
        {
            HttpResponse ReturnObject = new HttpResponse(new Version(1, 1), HttpStatusCode.OK);
            ReturnObject.ProcessHeaders(Dls1ResponseHeaders);
            ReturnObject.ProcessBody(Response, ContentType);
            return ReturnObject;
        }
    }
}
