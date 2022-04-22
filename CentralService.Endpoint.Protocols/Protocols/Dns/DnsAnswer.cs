using CentralService.Utility.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Protocols.Dns
{
    public class DnsAnswer
    {
        public string DomainName { get; }
        public bool DomainNameSameAsQuestion { get; }
        public ushort QuestionType { get; }
        public ushort QuestionClass { get; }
        public uint Lifespan { get; }
        public ushort DataSize => Convert.ToUInt16(Data.Length);
        public byte[] Data { get; }
        public byte NameOffset { get; }

        public int Size => 10 + DataSize + (DomainNameSameAsQuestion ? 2 : DomainName.Length + 2);

        public DnsAnswer(DnsQuestion Question, bool DomainNameSameAsQuestion, byte[] Data, ushort Lifespan = 300)
        {
            NameOffset = (byte)(Question.Offset & 0xFF);
            DomainName = Question.DomainName;
            QuestionType = Question.QuestionType;
            QuestionClass = Question.QuestionClass;
            this.Lifespan = Lifespan;
            this.DomainNameSameAsQuestion = DomainNameSameAsQuestion;
            this.Data = Data;
        }

        public void WriteAnswer(BigEndianWriter Writer)
        {
            if (DomainNameSameAsQuestion)
                Writer.Write((ushort)(0xC000 | NameOffset));
            else
            {
                string[] SplitDomainName = DomainName.Split('.');
                for (int i = 0; i < SplitDomainName.Length; i++)
                {
                    Writer.Write((byte)SplitDomainName[i].Length);
                    Writer.Write(SplitDomainName[i]);
                }
                Writer.BaseStream.Position++;
            }
            Writer.Write(QuestionType);
            Writer.Write(QuestionClass);
            Writer.Write(Lifespan);
            Writer.Write(DataSize);
            Writer.Write(Data);
        }
    }
}
