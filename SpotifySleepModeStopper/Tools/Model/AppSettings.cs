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
        public bool IsScreenSleepEnabled { get; set; }
    }
}
