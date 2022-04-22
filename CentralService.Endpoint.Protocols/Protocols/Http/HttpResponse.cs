using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CentralService.Endpoint.Protocols.Protocols.Http
{
    public class HttpResponse
    {
        public Version HttpVersion { get; }
        public HttpStatusCode ResponseCode { get; }

        public IReadOnlyCollection<KeyValuePair<string, string>> Header => _Header;
        public byte[] Body { get; private set; }

        private List<KeyValuePair<string, string>> _Header = new List<KeyValuePair<string, string>>();

        public HttpResponse(Version HttpVersion, HttpStatusCode ResponseCode)
        {
            this.HttpVersion = HttpVersion;
            this.ResponseCode = ResponseCode;
        }

        public void ProcessHeaders(List<KeyValuePair<string, string>> Headers)
        {
            if (Headers != null)
                foreach (KeyValuePair<string, string> Property in Headers)
                    AddHeaderParameter(Property);
        }

        public void ProcessBody(byte[] Body, string ContentType)
        {
            if (Body != null)
            {
                AddHeaderParameter(new KeyValuePair<string, string>("Content-type", ContentType));
                AddHeaderParameter(new KeyValuePair<string, string>("Content-Length", Body.Length.ToString()));
                this.Body = Body;
            }
        }

        public byte[] GetBytes()
        {
            string HeaderString = $"HTTP/{ HttpVersion.Major }.{ HttpVersion.Minor } { (int)ResponseCode } { ResponseCode }\r\n{ ProtocolBase.GetResponseString(_Header, "\r\n", ": ") }\r\n\r\n";
            byte[] HeaderByteArray = Encoding.UTF8.GetBytes(HeaderString);
            if (Body == null)
                return HeaderByteArray;
            return HeaderByteArray.Concat(Body).ToArray();
        }

        private void AddHeaderParameter(KeyValuePair<string, string> Property)
        {
            KeyValuePair<string, string> ExistingProperty = _Header.FirstOrDefault(x => x.Key == Property.Key);
            if (ExistingProperty.Key != null)
                _Header.Remove(ExistingProperty);
            _Header.Add(Property);
        }
    }
}
