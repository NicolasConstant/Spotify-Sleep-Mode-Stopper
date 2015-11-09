using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using SpotifyTools.Contracts;

namespace SpotifyTools.Tools
{
    public class NAudioSoundAnalyser : ISoundAnalyser
    {
        public bool IsWindowsOutputingSound()
        {
            var enumerator = new MMDeviceEnumerator();

            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active);
            var devicesArray = devices.ToArray();

            var windowsIsOutputing = false;
            foreach (var mmDevice in devicesArray)
            {
                var value = (int)(Math.Round(mmDevice.AudioMeterInformation.MasterPeakValue * 100));
                if (value > 0) windowsIsOutputing = true;
            }
            return windowsIsOutputing;
        }

        public bool IsProcessNameOutputingSound(string processName)
        {
            throw new NotImplementedException();
        }
    }
}
