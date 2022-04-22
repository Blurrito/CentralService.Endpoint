using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Protocols.Http
{
    public class HttpRequest
    {
        public string RequestType { get; }
        public string EndpointUrl { get; }
        public string HttpVersion { get;  }

        public IReadOnlyCollection<KeyValuePair<string, string>> Header => _Header;
        public byte[] Body { get; }

        private List<KeyValuePair<string, string>> _Header;

        public HttpRequest(byte[] Request)
        {
            int TerminatorIndex = GetHeaderTerminatorIndex(Request);
            _Header = GetRequestHeader(Request, TerminatorIndex);
            Body = GetRequestBody(Request, TerminatorIndex + 4);

            string[] RequestInfo = GetRequestInformation();
            RequestType = RequestInfo[0];
            EndpointUrl = RequestInfo[1];
            HttpVersion = RequestInfo[2];
        }

        private string[] GetRequestInformation()
        {
            string[] RequestInfo = _Header[0].Key.Split(' ');
            if (RequestInfo.Length != 3)
                throw new ArgumentException("Incomplete or corrupted request header.");
            _Header.RemoveAt(0);
            return RequestInfo;
        }

        private List<KeyValuePair<string, string>> GetRequestHeader(byte[] Request, int TerminatorIndex)
        {
            byte[] Header = new byte[TerminatorIndex];
            Array.Copy(Request, 0, Header, 0, TerminatorIndex);
            string HeaderString = Encoding.UTF8.GetString(Header);
            List<KeyValuePair<string, string>> HeaderParameters = ProtocolBase.GetRequestProperties(HeaderString, "\r\n", ": ");
            return HeaderParameters;
        }

        private byte[] GetRequestBody(byte[] Request, int BodyStartIndex)
        {
            KeyValuePair<string, string> ContentLength = _Header.FirstOrDefault(x => x.Key == "Content-Length");
            if (ContentLength.Key == null)
                return null;
            int BodySize = 0;
            if (!int.TryParse(ContentLength.Value, out BodySize))
                throw new Exception("Content length provided by the request headers is not a number.");
            if (Request.Length - BodyStartIndex - 1 < BodySize)
                throw new ArgumentException("The body of the request is shorter than the length set in the header.", nameof(Request));
            byte[] RequestBody = new byte[BodySize];
            Array.Copy(Request, BodyStartIndex, RequestBody, 0, BodySize);
            return RequestBody;
        }

        private int GetHeaderTerminatorIndex(byte[] Request)
        {
            int CurrentIndex = 0;
            int TerminatorIndex = -1;
            while (CurrentIndex <= Request.Length - 4 && TerminatorIndex == -1)
            {
                if (Request[CurrentIndex] == 0x0D && Request[CurrentIndex + 1] == 0x0A &&
                    Request[CurrentIndex + 2] == 0x0D && Request[CurrentIndex + 3] == 0x0A)
                    TerminatorIndex = CurrentIndex;
                CurrentIndex++;
            }
            if (TerminatorIndex == -1)
                throw new ArgumentException("The provided request is incomplete or corrupted.", nameof(Request));
            return TerminatorIndex;
        }
    }
}
