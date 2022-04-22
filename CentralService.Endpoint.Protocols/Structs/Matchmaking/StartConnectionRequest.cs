using CentralService.Utility.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Matchmaking
{
    public struct StartConnectionRequest
    {
        public int Address { get; set; }
        public ushort Port { get; set; }
        public byte[] Message { get; set; }

        public StartConnectionRequest(ServerSearchRequest Request)
        {
            using BigEndianReader Reader = new BigEndianReader(new MemoryStream(Request.Data));
            Address = Reader.ReadInt32(false);
            Port = Reader.ReadUInt16();
            Message = Reader.ReadBytes(Request.Length - 9);
        }
    }
}
