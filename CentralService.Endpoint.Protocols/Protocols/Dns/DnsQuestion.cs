using CentralService.Utility.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Protocols.Dns
{
    public class DnsQuestion
    {
        public string DomainName { get; }
        public ushort QuestionType { get; }
        public ushort QuestionClass { get; }
        public long Offset { get; }

        public int Size => DomainName.Length + 6;

        public DnsQuestion(BigEndianReader Reader)
        {
            Offset = Reader.BaseStream.Position;
            int LetterCount = Reader.ReadByte();
            byte[] Buffer = Reader.ReadBytes(LetterCount);
            DomainName = Encoding.UTF8.GetString(Buffer);
            while ((LetterCount = Reader.ReadByte()) > 0)
                DomainName += $".{ Encoding.UTF8.GetString(Reader.ReadBytes(LetterCount)) }";
            QuestionType = Reader.ReadUInt16();
            QuestionClass = Reader.ReadUInt16();
        }

        public void WriteQuestion(BigEndianWriter Writer)
        {
            string[] SplitDomainName = DomainName.Split('.');
            for (int i = 0; i < SplitDomainName.Length; i++)
                Writer.Write(SplitDomainName[i]);
            Writer.BaseStream.Position++;
            Writer.Write(QuestionType);
            Writer.Write(QuestionClass);
        }
    }
}
