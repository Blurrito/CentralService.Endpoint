using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.Protocols.Protocols.Http;
using CentralService.Endpoint.Interfaces.Protocols;

namespace CentralService.Endpoint.Protocols
{
    public class ConnectionTestServer : ITcpServer
    {
        private TcpClient _Client;
        private NetworkStream _Stream;

        private readonly List<KeyValuePair<string, string>> ConnectionTestResponseHeaders = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("X-Organization", "Nintendo"),
                new KeyValuePair<string, string>("Server", "BigIP"),
                new KeyValuePair<string, string>("Connection", "close")
            };
        private readonly string ConnectionTestResponseBody = $"<!DOCTYPE html PUBLIC \" -//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html>\r\n\r\n<head>\r\n		<title>HTML Page</title>\r\n</head>\r\n\r\n<body bgcolor=\"#FFFFFF\">\r\nThis is test.html page\r\n</body>\r\n\r\n</html>\r\n";

        public async Task HandleClient(TcpClient Client, Stream Stream)
        {
            _Client = Client;
            _Stream = (NetworkStream)Stream;

            while (_Client.Available < 1)
                await Task.Delay(100);
            byte[] RequestData = await Read();
            HttpRequest Request = new HttpRequest(RequestData);
            await Write();
        }

        public void Dispose()
        {
            _Stream.Close();
            _Stream.Dispose();
            _Client.Close();
        }

        private async Task<byte[]> Read()
        {
            byte[] Request = new byte[_Client.Available];
            await _Stream.ReadAsync(Request, 0, _Client.Available);
            return Request;
        }

        private async Task Write()
        {
            HttpResponse ResponseObject = new HttpResponse(new Version(1, 0), System.Net.HttpStatusCode.OK);
            ResponseObject.ProcessHeaders(ConnectionTestResponseHeaders);
            ResponseObject.ProcessBody(Encoding.UTF8.GetBytes(ConnectionTestResponseBody), "text/http");
            byte[] ResponseByteArray = ResponseObject.GetBytes();
            await _Stream.WriteAsync(ResponseByteArray, 0, ResponseByteArray.Length);
        }
    }
}
