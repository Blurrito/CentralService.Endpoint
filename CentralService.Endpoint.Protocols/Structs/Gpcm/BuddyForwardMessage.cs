using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct BuddyForwardMessage
    {
        public int bm { get; set; }
        public int f { get; set; }
        public string date { get; set; }
        public string msg { get; set; }

        public BuddyForwardMessage(int SenderId, BuddyMessage Message)
        {
            bm = Message.bm;
            f = SenderId;
            date = Message.date;
            msg = Message.msg;
        }

        public BuddyForwardMessage(int Type, int SenderId, string Date, string Message)
        {
            bm = Type;
            f = SenderId;
            date = Date;
            msg = Message;
        }
    }
}
