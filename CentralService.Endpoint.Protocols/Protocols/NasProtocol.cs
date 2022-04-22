using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Protocols
{
    public static class NasProtocol
    {
        public static string GetRequestType(string Request)
        {
            if (Request == string.Empty)
                throw new ArgumentException("Provided request does not contain any data.", nameof(Request));
            string[] Property = Request.Split('&')[0].Split('=');
            if (Property.Length != 2)
                throw new ArgumentException("Provived request is invalid or corrupted", nameof(Request));
            return ProtocolBase.FromBase64String(Property[1]);
        }

        public static TType Deserialize<TType>(string Input) where TType : struct
        {
            if (Input == string.Empty)
                throw new ArgumentException("Provided string does not contain any data.", nameof(Input));
            List<KeyValuePair<string, string>> RequestProperties = ProtocolBase.GetRequestProperties(Input, "&", "=", true);
            return ProtocolBase.SetStructProperties<TType>(RequestProperties);
        }

        public static string Serialize<TType>(TType Input) where TType : struct
        {
            List<KeyValuePair<string, string>> ResponseProperties = ProtocolBase.GetStructProperties(Input);
            return ProtocolBase.GetResponseString(ResponseProperties, "&", "=", true) + "\r\n";
        }
    }
}
