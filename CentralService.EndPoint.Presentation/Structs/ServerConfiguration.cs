using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CentralService.EndPoint.Presentation.Enums;
using CentralService.Factories.Servers.Enums;

namespace CentralService.Endpoint.Presentation.Structs
{
    public class ServerConfiguration
    {
        public string DomainName { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }
        public string ServerCertificateName { get; set; }
        public string[] AdditionalCertificateNames { get; set; }
        public ListenerTypes ListenerType { get; set; }
        public ServerTypes ServerType { get; set; }
    }
}
