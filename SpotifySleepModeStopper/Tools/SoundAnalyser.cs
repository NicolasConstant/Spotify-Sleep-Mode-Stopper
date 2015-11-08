using System;
using System.Diagnostics;
using CSCore.CoreAudioAPI;
using SpotifyTools.Contracts;

namespace SpotifyTools.Tools
{
    public class SoundAnalyser : ISoundAnalyser
    {
        public bool IsWindowsOutputingSound()
        {
            return IsOutputingSound();
        }

        public bool IsProcessNameOutputingSound(string processName)
        {
            return IsOutputingSound(processName);
        }

        private bool IsOutputingSound(string limitToProcess = null)
        {
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
            {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
                {
                    foreach (var session in sessionEnumerator)
                    {
                        using (var audioMeterInformation = session.QueryInterface<AudioMeterInformation>())
                        using (var session2 = session.QueryInterface<AudioSessionControl2>())
                        {
                            if (limitToProcess != null)
                            {
                                var processId = session2.ProcessID;
                                var name = Process.GetProcessById(processId).ProcessName;
                                if (name != limitToProcess) continue;
                            }

                            var peakValue = Math.Abs(audioMeterInformation.GetPeakValue());
                            if (peakValue >= 5E-10) return true;
                        }
                    }
                }
            }
            return false;
        }
            

        private AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
                {
                    var sessionManager = AudioSessionManager2.FromMMDevice(device);
                    return sessionManager;
                }
            }
        }
    }
}
