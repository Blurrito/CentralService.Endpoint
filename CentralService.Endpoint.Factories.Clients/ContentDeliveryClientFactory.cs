using CentralService.Endpoint.Client;
using CentralService.Endpoint.Interfaces.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Factories.Clients
{
    public static class ContentDeliveryClientFactory
    {
        public static IContentDeliveryClient GetClient() => new ContentDeliveryClient("/api/ds/cdn");
    }
}
