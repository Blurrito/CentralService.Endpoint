using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CentralService.Endpoint.Interfaces.Clients;
using CentralService.Endpoint.Client;

namespace CentralService.Endpoint.Factories.Clients
{
    public static class DeviceProfileClientFactory
    {
        public static IDeviceProfileClient GetClient() => new DeviceProfileClient("/api/ds/device");
    }
}
