using CentralService.Utility.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Protocols.Dns
{
    public class DnsResponse
    {
        public ushort TransactionId { get; }
        public bool IsResponse { get; }
        public byte QueryType { get; }
        public bool IsAuthoritative { get; }
        public bool IsTruncated { get; }
        public bool RecursionDesired { get; }
        public bool RecursionAvailable { get; }
        public bool AnswerAuthenticated { get; }
        public bool AllowNonAuthenticatedData { get; }
        public byte ResponseCode { get; }
        public ushort QuestionCount => Convert.ToUInt16(_Questions.Count);
        public ushort AnswerCount => Convert.ToUInt16(_Answers.Count);
        public ushort AuthorityMessageCount { get; }
        public ushort AdditionalMessageCount { get; }
        public IReadOnlyCollection<DnsQuestion> Questions => _Questions;
        public IReadOnlyCollection<DnsAnswer> Answers => _Answers;

        private List<DnsQuestion> _Questions = new List<DnsQuestion>();
        private List<DnsAnswer> _Answers = new List<DnsAnswer>();

        public DnsResponse(DnsRequest Request, bool IsAuthoritative = true, bool RecursionAvailable = false)
        {
            TransactionId = Request.TransactionId;
            IsResponse = true;
            QueryType = Request.QueryType;
            this.IsAuthoritative = IsAuthoritative;
            IsTruncated = Request.IsTruncated;
            RecursionDesired = Request.RecursionDesired;
            this.RecursionAvailable = RecursionAvailable;
            AnswerAuthenticated = false;
            AllowNonAuthenticatedData = Request.AllowNonAuthenticatedData;
            ResponseCode = 0;
            foreach (DnsQuestion Question in Request.Questions)
                _Questions.Add(Question);
        }

        public byte[] GetBytes()
        {
            byte[] Buffer = new byte[12 + _Questions.Sum(x => x.Size) + _Answers.Sum(x => x.Size)];
            using (BigEndianWriter Writer = new BigEndianWriter(new MemoryStream(Buffer)))
            {
                Writer.Write(TransactionId);
                WriteFlags(Writer);
                Writer.Write(QuestionCount);
                Writer.Write(AnswerCount);
                Writer.Write(AuthorityMessageCount);
                Writer.Write(AdditionalMessageCount);
                foreach (DnsQuestion Question in _Questions)
                    Question.WriteQuestion(Writer);
                foreach (DnsAnswer Answer in _Answers)
                    Answer.WriteAnswer(Writer);
            }
            return Buffer;
        }

        public void AddDnsAnswer(DnsAnswer Answer)
        {
            if (Answer != null)
                _Answers.Add(Answer);
        }

        private void WriteFlags(BigEndianWriter Writer)
        {
            ushort Flags = 0;
            Flags |= (ushort)((Convert.ToByte(IsResponse) & 0x1) << 15);
            Flags |= (ushort)((QueryType & 0xF) << 11);
            Flags |= (ushort)((Convert.ToByte(IsAuthoritative) & 0x1) << 10);
            Flags |= (ushort)((Convert.ToByte(IsTruncated) & 0x1) << 9);
            Flags |= (ushort)((Convert.ToByte(RecursionDesired) & 0x1) << 8);
            Flags |= (ushort)((Convert.ToByte(RecursionAvailable) & 0x1) << 7);
            Flags |= (ushort)((Convert.ToByte(AnswerAuthenticated) & 0x1) << 5);
            Flags |= (ushort)((Convert.ToByte(AllowNonAuthenticatedData) & 0x1) << 4);
            Flags |= (ushort)(ResponseCode & 0xF);
            Writer.Write(Flags);
        }
    }
}
