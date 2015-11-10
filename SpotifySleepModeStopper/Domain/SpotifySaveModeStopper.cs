using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SpotifyTools.Contracts;
using SpotifyTools.Tools;

namespace SpotifyTools.Domain
{
    public class SpotifySaveModeStopper
    {
        private const string SpotifyProcessName = "Spotify";

        private readonly IMessageDisplayer _messageDisplayer;
        private readonly IPreventSleepScreen _preventSleepScreen;
        private readonly ISoundAnalyser _soundAnalyser;

        private bool _spotifyRunning;
        private bool _spotifyPlaying;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _analyst;

        public SpotifySaveModeStopper(IMessageDisplayer messageDisplayer, IPreventSleepScreen preventSleepScreen, ISoundAnalyser soundAnalyser)
        {
            _messageDisplayer = messageDisplayer;
            _preventSleepScreen = preventSleepScreen;
            _soundAnalyser = soundAnalyser;
        }

        public void StartListening()
        {
            _messageDisplayer.OutputMessage("Start listening");
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _analyst = Repeat.Interval(
                    TimeSpan.FromSeconds(10),
                    AnalyseSpotifyStatus, _cancellationTokenSource.Token);
        }

        public void StopListening()
        {
            _messageDisplayer.OutputMessage("Stop listening");
            _cancellationTokenSource?.Cancel();

            //Make sure sleep screen is enabled again
            _preventSleepScreen.EnableConstantDisplayAndPower(false);
        }

        private void AnalyseSpotifyStatus()
        {
            try
            {
                var stateHasChanged = false;

                var isSpotifyRunning = IsSpotifyRunning();
                if (_spotifyRunning != isSpotifyRunning)
                {
                    stateHasChanged = true;
                    _spotifyRunning = isSpotifyRunning;
                    _messageDisplayer.OutputMessage("Spotify Running: " + _spotifyRunning);
                }

                if (_spotifyRunning)
                {
                    var isSpotifyPlaying = IsSoundStreaming();
                    if (_spotifyPlaying != isSpotifyPlaying)
                    {
                        stateHasChanged = true;
                        _spotifyPlaying = isSpotifyPlaying;
                        _messageDisplayer.OutputMessage("Spotify Playing: " + _spotifyPlaying);
                    }
                }

                if (stateHasChanged)
                {
                    if (_spotifyRunning && _spotifyPlaying)
                        _preventSleepScreen.EnableConstantDisplayAndPower(true);
                    else
                        _preventSleepScreen.EnableConstantDisplayAndPower(false);
                }
            }
            catch (Exception e)
            {
                _messageDisplayer.OutputMessage("Exception: " + e.Message + " at " + e.StackTrace);
            }
        }

        private bool IsSpotifyRunning()
        {
            return Process.GetProcessesByName(SpotifyProcessName).Any();
        }

        private bool IsSoundStreaming()
        {
            var isSpotifyPlaying = _soundAnalyser.IsProcessNameOutputingSound(SpotifyProcessName);
            //var isSpotifyPlaying = _soundAnalyser.IsWindowsOutputingSound();
            return isSpotifyPlaying;
        }
    }
}


