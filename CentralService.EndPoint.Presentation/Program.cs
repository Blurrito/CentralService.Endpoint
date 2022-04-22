using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using CentralService.Endpoint.Interfaces.Listeners;
using CentralService.Endpoint.Presentation.Structs;
using CentralService.EndPoint.Presentation.Factories;
using System.Linq;
using CentralService.Utility;
using System.Text;
using System.Security.Cryptography;

namespace CentralService.EndPoint.Presentation
{
    class Program
    {
        private static List<IListener> Listeners = new List<IListener>();

        static void Main(string[] args)
        {
            GetServers(GetServerConfigurationList());
            Console.WriteLine("Starting...");
            foreach (IListener Listener in Listeners)
                Listener.Start();
            Console.ReadLine();
            Console.WriteLine("Stopping...");
            foreach (IListener Listener in Listeners)
                Listener.Stop();
        }

        private static List<ServerConfiguration> GetServerConfigurationList()
        {
            string SerializedList;
            using (StreamReader Reader = new StreamReader(new FileStream("D:\\Nintendo\\DS\\server\\ServerConfigurationList.json", FileMode.Open, FileAccess.Read)))
                SerializedList = Reader.ReadToEnd();
            List<ServerConfiguration> ConfigurationList = JsonConvert.DeserializeObject<List<ServerConfiguration>>(SerializedList);

            foreach (ServerConfiguration Configuration in ConfigurationList)
                if (Configuration.Address == "LOCAL")
                    Configuration.Address = Utilities.GetLocalAddress().ToString();
            return ConfigurationList;
        }

        private static void GetServers(List<ServerConfiguration> ConfigurationList)
        {
            foreach (ServerConfiguration Configuration in ConfigurationList)
                Listeners.Add(ListenerFactory.GetListener(Configuration));
        }
    }
}
