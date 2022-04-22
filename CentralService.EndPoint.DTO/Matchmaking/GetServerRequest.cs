using CentralService.Utility.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Matchmaking
{
    public struct GetServerRequest
    {
        public byte ListVersion { get; set; }
        public byte EncodingVersion { get; set; }
        public int GameVersion { get; set; }
        public string GameName { get; set; }
        public string ServiceName { get; set; }
        public string ValidationString { get; set; }
        public string Filters { get; set; }
        public string Fields { get; set; }
        public int Flags { get; set; }
        public byte[] ExtraData { get; set; }

        public GetServerRequest(byte[] Request)
        {
            using (BigEndianReader Reader = new BigEndianReader(new MemoryStream(Request)))
            {
                ListVersion = Reader.ReadByte();
                EncodingVersion = Reader.ReadByte();
                GameVersion = Reader.ReadInt32();
                GameName = Reader.ReadString();
                ServiceName = Reader.ReadString();
                ValidationString = Reader.ReadString(8);
                Filters = Reader.ReadString();
                Fields = Reader.ReadString();
                Flags = Reader.ReadInt32();
                ExtraData = Reader.ReadBytes(Request.Length - (int)Reader.BaseStream.Position);
            }
        }
    }
}
