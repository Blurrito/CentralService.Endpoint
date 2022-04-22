using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.DTO.Matchmaking;
using CentralService.Endpoint.Protocols.Protocols.Dns;
using CentralService.Endpoint.Interfaces.Protocols;
using CentralService.Utility;

namespace CentralService.Endpoint.Protocols
{
    public class DnsServer : IUdpServer
    {
        private readonly List<KeyValuePair<string, IPAddress>> _DnsList = new List<KeyValuePair<string, IPAddress>>();

        public DnsServer()
        {
            ResourceSet Set =  Properties.Resources.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            if (Set != null)
                foreach (DictionaryEntry Entry in Set)
                    if (Entry.Value.ToString() == "LOCAL")
                        _DnsList.Add(new KeyValuePair<string, IPAddress>(Entry.Key.ToString().Replace('_', '.'), Utilities.GetLocalAddress()));
                    else
                        _DnsList.Add(new KeyValuePair<string, IPAddress>(Entry.Key.ToString().Replace('_', '.'), IPAddress.Parse(Entry.Value.ToString())));
        }

        public async Task<byte[]> HandleClient(IPAddress Address, short Port, byte[] Data)
        {
            try
            { 
                DnsRequest Request = new DnsRequest(Data);
                DnsResponse Response = ProcessRequest(Request);
                return Response.GetBytes();
            }
            catch (Exception Ex)
            {
                Console.WriteLine($"Dns Server - Exception => { Ex.Message }");
                return null;
            }
        }

        public List<MatchmakingMessage> GetPendingMessages() => new List<MatchmakingMessage>();

        public void Dispose() { }

        private DnsResponse ProcessRequest(DnsRequest Request)
        {
            DnsResponse Response = new DnsResponse(Request);
            switch (Request.QueryType)
            {
                case 0:
                    ProcessStandardQuery(Response);
                    break;
                default:
                    throw new NotImplementedException($"Received request containing an unsupported query type ({ Request.QueryType }).");
            }
            return Response;
        }

        private void ProcessStandardQuery(DnsResponse Response)
        {
            foreach (DnsQuestion Question in Response.Questions)
            {
                object DataObject = ProcessClass(Question);
                if (DataObject == null)
                    throw new ArgumentException(nameof(Question), $"Could not retrieve the IP address for { Question.DomainName }.");
                byte[] Data = ProcessType(Question, DataObject);
                Response.AddDnsAnswer(new DnsAnswer(Question, true, Data));
            }
        }

        private object ProcessClass(DnsQuestion Question)
        {
            switch (Question.QuestionClass)
            {
                case 1:
                    return _DnsList.FirstOrDefault(x => Question.DomainName.Contains(x.Key)).Value;
                default:
                    throw new NotImplementedException($"Received request containing an unsupported question class ({ Question.QuestionClass })");
            }
        }

        private byte[] ProcessType(DnsQuestion Question, object Data)
        {
            switch (Question.QuestionType)
            {
                case 1:
                    IPAddress Address = (IPAddress)Data;
                    return Address.GetAddressBytes();
                default:
                    throw new NotImplementedException($"Received request containing an unsupported question type ({ Question.QuestionType })");
            }
        }
    }
}
