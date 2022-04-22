using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Protocols
{
    public static class GamespyTcpProtocol
    {
        public static List<List<KeyValuePair<string, string>>> GetRequests(string Request)
        {
            if (Request == string.Empty)
                throw new ArgumentException("Provided string does not contain any data.");
            if (Request[0] == '\\')
                Request.Remove(0, 1);

            string[] SplitRequests = Request.Split("\\final\\");
            if (SplitRequests.Length < 2)
                throw new ArgumentException("Request incomplete or corrupted.", nameof(Request));
            List<List<KeyValuePair<string, string>>> ReturnList = new List<List<KeyValuePair<string, string>>>();
            for (int i = 0; i < SplitRequests.Length - 1; i++)
                ReturnList.Add(ProtocolBase.GetRequestProperties(SplitRequests[i].Substring(1), "\\", "\\"));
            return ReturnList;
        }

        public static TType Deserialize<TType>(List<KeyValuePair<string, string>> Properties) where TType : struct => ProtocolBase.SetStructProperties<TType>(Properties);

        public static string Serialize<TType>(TType Object) where TType : struct
        {
            List<KeyValuePair<string, string>> Properties = ProtocolBase.GetStructProperties(Object);
            return $"\\{ ProtocolBase.GetResponseString(Properties, "\\", "\\", false, true) }\\final\\";
        }
    }
}
