using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.User
{
    public struct DeviceProfile
    {
        public int DeviceProfileId { get; set; }
        public long DeviceId { get; set; }
        public string Password { get; set; }
        public string MacAddress { get; set; }
        public string DeviceName { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<GameProfile> GameProfiles { get; set; }

        public DeviceProfile(long DeviceId, string Password, string MacAddress, string DeviceName, GameProfile? Profile)
        {
            DeviceProfileId = 0;
            this.DeviceId = DeviceId;
            this.Password = Password;
            this.MacAddress = MacAddress;
            //TODO: Convert this from Unicode to UTF8;
            this.DeviceName = DeviceName;
            CreatedDate = DateTime.Now;
            GameProfiles = new List<GameProfile>();
            if (Profile != null)
                GameProfiles.Add(Profile.Value);
        }

        //public DeviceProfile(int DeviceProfileId, long DeviceId, string Password, string DeviceName, DateTime CreatedDate, List<GameProfile> GameProfiles)
        //{
        //    this.DeviceProfileId = DeviceProfileId;
        //    this.DeviceId = DeviceId;
        //    this.Password = Password;
        //    this.DeviceName = DeviceName;
        //    this.CreatedDate = CreatedDate;
        //    this.GameProfiles = GameProfiles;
        //}
    }
}
