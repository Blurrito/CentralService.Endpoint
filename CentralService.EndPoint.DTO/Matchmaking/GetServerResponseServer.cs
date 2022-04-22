using CentralService.Utility.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Matchmaking
{
    public struct GetServerResponseServer
    {
        public List<string> FieldValues { get; set; }
        public byte Flags { get; set; }
        public int PublicAddress { get; set; }
        public ushort PublicPort { get; set; }
        public string LocalAddress { get; set; }
        public ushort LocalPort { get; set; }
        public string ActualAddress { get; set; }
        public int SessionId { get; set; }

        public bool ContainsLocalIp => (Flags & 0x2) != 0;
        public bool AllowsNatnegCommunication => (Flags & 0x4) != 0;
        public bool ContainsIMCPAddress => (Flags & 0x8) != 0;
        public bool ContainsNonStandardPublicPort => (Flags & 0x10) != 0;
        public bool ContainsNonStandardLocalPort => (Flags & 0x20) != 0;
        public bool ContainsKeys => (Flags & 0x40) != 0;

        public int TotalLength => 5
                + (ContainsNonStandardPublicPort ? 2 : 0)
                + (ContainsLocalIp ? 4 : 0)
                + (ContainsNonStandardLocalPort ? 2 : 0)
                + (ContainsIMCPAddress ? 4 : 0)
                + FieldValues.Sum(x => x.Length + 2);

        public void ToByteArray(BigEndianWriter Writer)
        {
            Writer.Write(Flags);
            Writer.Write(PublicAddress, false);
            if (ContainsNonStandardPublicPort)
                Writer.Write(PublicPort);
            if (ContainsLocalIp)
            {
                string[] SplitAddress = LocalAddress.Split('.');
                foreach (string AddressByte in SplitAddress)
                    Writer.Write(Convert.ToByte(AddressByte));
            }
            if (ContainsNonStandardLocalPort)
                Writer.Write(LocalPort);
            if (ContainsIMCPAddress)
                Writer.Write(new byte[] { 0x00, 0x00, 0x00, 0x00 });
            foreach (string Value in FieldValues)
            {
                Writer.Write((byte)255);
                Writer.Write(Encoding.UTF8.GetBytes($"{ Value }\0"));
            }
        }
    }
}
