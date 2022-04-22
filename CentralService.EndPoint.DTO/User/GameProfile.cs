using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.DTO.User
{
    public struct GameProfile
    {
        public int GameProfileId { get; set; }
        public int DeviceProfileId { get; set; }
        public string GameCode { get; set; }
        public string Gsbrcd { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Nickname { get; set; }
        public string UniqueNickname { get; set; }
        public string Zipcode { get; set; }
        public string Aim { get; set; }
        public string Signature { get; set; }
        public int Pid { get; set; }
        public float Longnitude { get; set; }
        public float Lattitude { get; set; }
        public string Location { get; set; }

        public GameProfile(long DeviceId, string GameCode, string Gsbrcd)
        {
            GameProfileId = 0;
            DeviceProfileId = 0;
            this.GameCode = GameCode;
            this.Gsbrcd = Gsbrcd;

            UniqueNickname = $"{ Base32Encode(DeviceId) }{ Gsbrcd }";
            Nickname = UniqueNickname;
            Email = $"{ UniqueNickname }@nds";
            Signature = new string('0', 32);
            Pid = 11;
            Longnitude = 0f;
            Lattitude = 0f;
            Location = string.Empty;

            FirstName = null;
            LastName = null;
            Zipcode = null;
            Aim = null;
        }

        public GameProfile(int GameProfileId, string Nickname, string FirstName, string LastName, string Zipcode, string Aim, float Longnitude, float Lattitude, string Location)
        {
            this.GameProfileId = GameProfileId;
            DeviceProfileId = 0;
            GameCode = null;
            Gsbrcd = null;
            UniqueNickname = null;
            Email = null;
            Signature = null;
            Pid = 0;

            this.Nickname = Nickname;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.Zipcode = Zipcode;
            this.Aim = Aim;
            this.Longnitude = Longnitude;
            this.Lattitude = Lattitude;
            this.Location = Location;
        }

        /// <summary>
        /// Converts an integer to a base32 encoded string.
        /// Courtesy to dwc server emulator project (https://github.com/barronwaffles/dwc_network_server_emulator) for this!
        /// </summary>
        /// <param name="Input">The input integer.</param>
        /// <param name="Reverse">Determines whether or not the result string should be reversed before being returned (Default is true).</param>
        /// <returns>The provided integer converted to a base32 encoded string.</returns>
        private static string Base32Encode(long Input, bool Reverse = true)
        {
            string CharSet = "0123456789abcdefghijklmnopqrstuv";
            string Encoded = "";

            while (Input > 0)
            {
                Encoded += CharSet[(int)(Input & 0x1F)];
                Input >>= 5;
            }

            if (Reverse)
            {
                char[] Buffer = Encoded.ToCharArray();
                Array.Reverse(Buffer);
                Encoded = new string(Buffer);
            }
            return Encoded;
        }
    }
}
