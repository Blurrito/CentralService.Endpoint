using CentralService.Endpoint.DTO.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Structs.Gpcm
{
    public struct ProfileInfo
    {
        public string pi => string.Empty;
        public int profileid { get; set; }
        public string nick { get; set; }
        public int userid { get; set; }
        public string email { get; set; }
        public string sig { get; set; }
        public string uniquenick { get; set; }
        public int pid { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string zipcode { get; set; }
        public string aim { get; set; }
        public string lon { get; set; }
        public string lat { get; set; }
        public string loc { get; set; }
        public int id { get; set; }

        public ProfileInfo(GameProfile Profile, int MessageId)
        {
            profileid = Profile.GameProfileId;
            nick = Profile.Nickname;
            userid = Profile.DeviceProfileId;
            email = Profile.Email;
            sig = Profile.Signature;
            uniquenick = Profile.UniqueNickname;
            pid = Profile.Pid;
            firstname = Profile.FirstName;
            lastname = Profile.LastName;
            zipcode = Profile.Zipcode;
            aim = Profile.Aim;
            lon = Profile.Longnitude.ToString("0.000000").Replace(',', '.');
            lat = Profile.Lattitude.ToString("0.000000").Replace(',', '.');
            loc = Profile.Location;
            id = MessageId;
        }
    }
}
