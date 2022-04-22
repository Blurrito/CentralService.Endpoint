using CentralService.Utility.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Protocols.Dns
{
    public class DnsRequest
    {
        public ushort TransactionId { get; }
        public bool IsResponse { get; }
        public byte QueryType { get; }
        public bool IsTruncated { get; }
        public bool RecursionDesired { get; }
        public bool AllowNonAuthenticatedData { get; }
        public ushort QuestionCount { get; }
        public ushort AnswerCount { get; }
        public ushort AuthorityMessageCount { get; }
        public ushort AdditionalMessageCount { get; }
        public IReadOnlyCollection<DnsQuestion> Questions => _Questions;
        public IReadOnlyCollection<DnsAnswer> Answers => _Answers;

        private List<DnsQuestion> _Questions = new List<DnsQuestion>();
        private List<DnsAnswer> _Answers = new List<DnsAnswer>();

        public DnsRequest(byte[] Request)
        {
            if (Request.Length < 12)
                throw new ArgumentException(nameof(Request), "Incoming request is malformed or corrupted.");
            using (BigEndianReader Reader = new BigEndianReader(new MemoryStream(Request)))
            {
                TransactionId = Reader.ReadUInt16();
                ushort Flags = Reader.ReadUInt16();
                IsResponse = Convert.ToBoolean(Flags >> 15);
                QueryType = (byte)((Flags >> 11) & 0xF);
                IsTruncated = Convert.ToBoolean(Flags >> 9);
                RecursionDesired = Convert.ToBoolean(Flags >> 8);
                AllowNonAuthenticatedData = Convert.ToBoolean(Flags >> 4);
                QuestionCount = Reader.ReadUInt16();
                AnswerCount = Reader.ReadUInt16();
                AuthorityMessageCount = Reader.ReadUInt16();
                AdditionalMessageCount = Reader.ReadUInt16();

                for (int i = 0; i < QuestionCount; i++)
                    _Questions.Add(new DnsQuestion(Reader));
            }
        }
    }
}
