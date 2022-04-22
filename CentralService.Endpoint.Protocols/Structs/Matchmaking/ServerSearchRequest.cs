using CentralService.Utility;
using CentralService.Utility.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Matchmaking
{
    public struct ServerSearchRequest
    {
        public short Length { get; set; }
        public byte Type { get; set; }
        public byte[] Data { get; set; }
        public int ActualLength => 3 + Data.Length;

        public ServerSearchRequest(byte[] Request)
        {
            using (BigEndianReader Reader = new BigEndianReader(new MemoryStream(Request)))
            {
                Length = Reader.ReadInt16();
                Type = Reader.ReadByte();
                Data = Reader.ReadBytes(Length - 3);
            }
        }
    }
}
