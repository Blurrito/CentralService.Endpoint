using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.Authentication
{
    public struct Challenge
    {
        public string Address { get; set; }
        public string Token { get; set; }
        public string NasChallenge { get; set; }
        public string GpcmChallenge { get; set; }
        public int DeviceProfileId { get; set; }
        public int GameProfileId { get; set; }
        public string UniqueNickname { get; set; }
        public DateTime ValidUntil { get; set; }
    }
}
