using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.User
{
    public struct Buddy
    {
        public int BuddyId { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        public int Status { get; set; }
        public long Date { get; set; }

        public Buddy(int SenderId, int RecipientId, int Status = 0)
        {
            BuddyId = 0;
            this.SenderId = SenderId;
            this.RecipientId = RecipientId;
            this.Status = Status;
            Date = Convert.ToInt64(DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }

        public Buddy(Buddy Base, int NewStatus)
        {
            BuddyId = Base.BuddyId;
            SenderId = Base.SenderId;
            RecipientId = Base.RecipientId;
            Status = NewStatus;
            Date = Base.Date;
        }
    }
}
