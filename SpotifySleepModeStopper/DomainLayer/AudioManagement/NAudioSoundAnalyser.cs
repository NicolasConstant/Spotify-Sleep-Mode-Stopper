using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using SpotifyTools.Contracts;

namespace SpotifyTools.DomainLayer.AudioManagement
{
    public class NAudioSoundAnalyser : ISoundAnalyser
    {
        private readonly IMessageDisplayer _messageDisplayer;

        #region Ctor
        public NAudioSoundAnalyser(IMessageDisplayer messageDisplayer)
        {
            _messageDisplayer = messageDisplayer;
        }
        #endregion

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
            try
            {
                using (var enumerator = new MMDeviceEnumerator())
                {
                    // Enumerate all active render (output) devices
                    var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
                    
                    foreach (var device in devices)
                    {
                        try
                        {
                            // Get the session manager for this device
                            var sessionManager = device.AudioSessionManager;
                            if (sessionManager == null) continue;

                            // Enumerate all sessions on this device
                            var sessionCount = sessionManager.Sessions.Count;
                            for (int i = 0; i < sessionCount; i++)
                            {
                                try
                                {
                                    var session = sessionManager.Sessions[i];
                                    if (session == null) continue;

                                    // Check if the session state is Active (value 1)
                                    if (session.State == AudioSessionState.AudioSessionStateActive)
                                    {
                                        if (limitToProcess != null)
                                        {
                                            // Get process ID and compare process name
                                            var processId = (int)session.GetProcessID;
                                            try
                                            {
                                                var process = Process.GetProcessById(processId);
                                                if (process.ProcessName != limitToProcess)
                                                    continue;
                                            }
                                            catch (ArgumentException)
                                            {
                                                // Process no longer exists, skip
                                                continue;
                                            }
                                        }

                                        return true;
                                    }
                                }
                                catch (Exception e)
                                {
                                    _messageDisplayer.OutputMessage("Session exception: " + e.Message);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _messageDisplayer.OutputMessage("Device exception: " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _messageDisplayer.OutputMessage("Enumerator exception: " + e.Message + " at " + e.StackTrace);
            }
            
            return false;
        }
    }
}
