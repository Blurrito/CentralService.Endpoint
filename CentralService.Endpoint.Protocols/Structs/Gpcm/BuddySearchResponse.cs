using CentralService.Endpoint.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct BuddySearchResponse
    {
        public string bsr => string.Empty;
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string uniquenick { get; set; }
        public int namespaceid => 16;
        public string bsrdone => string.Empty;

        public BuddySearchResponse(GameProfile BuddyProfile)
        {
            firstname = BuddyProfile.FirstName;
            lastname = BuddyProfile.LastName;
            email = BuddyProfile.Email;
            uniquenick = BuddyProfile.UniqueNickname;
        }
    }
}
