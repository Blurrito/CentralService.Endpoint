using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Interfaces.Protocols
{
    public interface ITcpServer : IDisposable
    {
        Task HandleClient(TcpClient Client, Stream Stream);
    }
}
