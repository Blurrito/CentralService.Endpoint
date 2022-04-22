using CentralService.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Authentication
{
    public struct MatchmakingChallenge
    {
        public int SessionId { get; set; }
        public string ChallengeString { get; set; }
        public IPAddress Address { get; set; }
        public short Port { get; set; }

        public MatchmakingChallenge(int SessionId, string ChallengeString, IPAddress Address, short Port)
        {
            this.SessionId = SessionId;
            this.ChallengeString = ChallengeString;
            this.Address = Address;
            this.Port = Port;
        }

        public byte[] ToByteArray()
        {
            byte[] Result = new byte[ChallengeString.Length + 22];
            using (BinaryWriter Writer = new BinaryWriter(new MemoryStream(Result)))
            {
                Writer.Write(new byte[] { 0xFE, 0xFD, 0x01 });
                Writer.Write(SessionId);
                Writer.Write(Encoding.UTF8.GetBytes(ChallengeString));
                Writer.Write(new byte[] { 0x30, 0x30 });

                Writer.Write(Encoding.UTF8.GetBytes(Utilities.AddressToHexadecimalString(Address)));
                Writer.Write(Encoding.UTF8.GetBytes(Utilities.PortToHexadecimalString(Port)));
            }
            return Result;
        }
    }
}
