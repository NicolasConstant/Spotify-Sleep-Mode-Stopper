using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpotifyTools.Tools.Model
{
    [DataContract]
    public class AppSettings
    {
        [DataMember]
        public string Version { get; set; } = "1.1";

        [DataMember]
        public bool IsScreenSleepEnabled { get; set; }

        [DataMember]
        public bool DonationMessageActive { get; set; }

        [DataMember]
        public bool ScreenLockActive { get; set; }
    }
}
