using System;
using Windows.Media.Control;
using SpotifyTools.Contracts;

namespace SpotifyTools.DomainLayer.AudioManagement
{
    public class WindowsMediaSessionDetector : IMediaSessionDetector
    {
        public bool IsMediaPlaying()
        {
            try
            {
                var manager = GlobalSystemMediaTransportControlsSessionManager
                    .RequestAsync()
                    .AsTask()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                var sessions = manager.GetSessions();
                foreach (var session in sessions)
                {
                    var playbackInfo = session.GetPlaybackInfo();
                    if (playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                // SMTC may be unavailable on older Windows or in restricted session states.
                // Fail silently and let the primary detection handle it.
            }

            return false;
        }
    }
}
