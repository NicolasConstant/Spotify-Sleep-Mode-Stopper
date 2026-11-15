using System;
using System.Threading;
using System.Threading.Tasks;
using SpotifyTools.Contracts;
using SpotifyTools.Tools;
using SpotifyTools.Tools.Model;

namespace SpotifyTools
{
    public class SpotifySaveModeStopperFacade : ISpotifySaveModeStopperFacade
    {
        private const string SpotifyProcessName = "Spotify";

        private readonly IMessageDisplayer _messageDisplayer;
        private readonly IPreventSleepScreen _preventSleepScreen;
        private readonly ISoundAnalyser _soundAnalyser;
        private readonly IAppStatusReporting _appState;
        private readonly IAutoStartManager _autoStartManager;
        private readonly ISettingsManager<AppSettings> _settingsManager;
        private readonly IProcessAnalyser _processAnalyser;
        private readonly IMediaSessionDetector _mediaSessionDetector;

        private bool _spotifyRunning;
        private bool _spotifyPlaying;
        private bool _screenSleepEnabled;
        private bool _isMediaSessionFallbackEnabled;

        private bool _appStateHasChanged = false;

        private CancellationTokenSource _cancellationTokenSource;
        private Task _analyst;

#if DEBUG
        TimeSpan _checkInterval = TimeSpan.FromSeconds(5);
#else
        TimeSpan _checkInterval = TimeSpan.FromSeconds(20);
#endif

        #region Ctor
        public SpotifySaveModeStopperFacade(IMessageDisplayer messageDisplayer, IPreventSleepScreen preventSleepScreen,
            ISoundAnalyser soundAnalyser, IProcessAnalyser processAnalyser, IAppStatusReporting appState, IAutoStartManager autoStartManager, ISettingsManager<AppSettings> settingsManager, IMediaSessionDetector mediaSessionDetector, int refreshRate = -1)
        {
            _messageDisplayer = messageDisplayer;
            _preventSleepScreen = preventSleepScreen;
            _soundAnalyser = soundAnalyser;
            _appState = appState;
            _autoStartManager = autoStartManager;
            _settingsManager = settingsManager;
            _processAnalyser = processAnalyser;
            _mediaSessionDetector = mediaSessionDetector;

            _screenSleepEnabled = IsScreenSleepEnabled();
            _isMediaSessionFallbackEnabled = IsMediaSessionFallbackEnabled();
            if(refreshRate > 0)
                _checkInterval = TimeSpan.FromSeconds(refreshRate);
        }
        #endregion

        public void ChangeScreenSleep(bool screenSleepEnabled)
        {
            _screenSleepEnabled = screenSleepEnabled;

            //If spotify already playing, change current sleep preventer 
            if (_spotifyPlaying)
                ResetListening();

            //Save new settings
            var settings = _settingsManager.GetConfig();
            settings.IsScreenSleepEnabled = screenSleepEnabled;
            _settingsManager.SaveConfig(settings);
        }

        public void ChangeAutoStart(bool autoStartEnabled)
        {
            _autoStartManager.SetAutoStart(autoStartEnabled);
        }

        public void ChangeMediaSessionFallback(bool mediaSessionFallbackEnabled)
        {
            _isMediaSessionFallbackEnabled = mediaSessionFallbackEnabled;

            //Save new settings
            var settings = _settingsManager.GetConfig();
            settings.IsMediaSessionFallbackEnabled = mediaSessionFallbackEnabled;
            _settingsManager.SaveConfig(settings);
        }

        public bool IsScreenSleepEnabled()
        {
            var settings = _settingsManager.GetConfig();
            return settings.IsScreenSleepEnabled;
        }

        public bool IsAutoStartEnabled()
        {
            return _autoStartManager.IsAutoStartSet();
        }

        public bool IsMediaSessionFallbackEnabled()
        {
            var settings = _settingsManager.GetConfig();
            return settings.IsMediaSessionFallbackEnabled;
        }

        public void StartListening()
        {
            _messageDisplayer.OutputMessage("Start listening");
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _analyst = Repeat.Interval(
                    _checkInterval,
                    () => AnalyseSpotifyStatus(_cancellationTokenSource.Token), 
                    _cancellationTokenSource.Token);
        }

        public void StopListening()
        {
            _messageDisplayer.OutputMessage("Stop listening");
            _cancellationTokenSource?.Cancel();

            //Make sure sleep mode is enabled again
            _preventSleepScreen.EnableConstantDisplayAndPower(false, false);
        }

        private void ResetListening()
        {
            _messageDisplayer.OutputMessage("Reset listening");

            StopListening();
            _appStateHasChanged = true;
            StartListening();
        }

        private void AnalyseSpotifyStatus(CancellationToken token)
        {
            try
            {
                if (!_appStateHasChanged)
                {
                    var isSpotifyRunning = IsSpotifyRunning();
                    if (_spotifyRunning != isSpotifyRunning)
                    {
                        _appStateHasChanged = true;
                        _spotifyRunning = isSpotifyRunning;
                        _messageDisplayer.OutputMessage("Spotify Running: " + _spotifyRunning);
                    }

                    var isMediaPlaying = IsMediaCurrentlyPlaying();
                    if (_spotifyPlaying != isMediaPlaying)
                    {
                        _appStateHasChanged = true;
                        _spotifyPlaying = isMediaPlaying;
                        _messageDisplayer.OutputMessage("Media Playing: " + _spotifyPlaying);
                    }
                }

                if (_appStateHasChanged)
                {
                    if (_spotifyPlaying)
                    {
                        _messageDisplayer.OutputMessage("Anti Sleep Mode is Enabled");
                        _appState.NotifyAntiSleepingModeIsActivated();

                        token.ThrowIfCancellationRequested();
                        _preventSleepScreen.EnableConstantDisplayAndPower(true, !_screenSleepEnabled);
                    }
                    else
                    {
                        _messageDisplayer.OutputMessage("Anti Sleep Mode is Disabled");
                        _appState.NotifyAntiSleepingModeIsDisabled();

                        token.ThrowIfCancellationRequested();
                        _preventSleepScreen.EnableConstantDisplayAndPower(false, false);
                    }

                    _appStateHasChanged = false;
                }
            }
            catch (Exception e)
            {
                _messageDisplayer.OutputMessage("Exception: " + e.Message + " at " + e.StackTrace);
            }
        }

        private bool IsSpotifyRunning()
        {
            var isSpotifyRunning = _processAnalyser.IsAppRunning(SpotifyProcessName);
            return isSpotifyRunning;
        }

        private bool IsSoundStreaming()
        {
            var isSpotifyPlaying = _soundAnalyser.IsProcessNameOutputingSound(SpotifyProcessName);
            return isSpotifyPlaying;
        }

        private bool IsMediaCurrentlyPlaying()
        {
            // Primary detection: the Spotify process is running and outputting sound.
            if (_spotifyRunning && IsSoundStreaming())
                return true;

            // Fallback detection: any player that integrates with Windows media keys
            // reports itself as playing. Only used when explicitly enabled.
            if (_isMediaSessionFallbackEnabled && _mediaSessionDetector != null)
            {
                try
                {
                    return _mediaSessionDetector.IsMediaPlaying();
                }
                catch (Exception e)
                {
                    _messageDisplayer.OutputMessage("Media session fallback exception: " + e.Message);
                }
            }

            return false;
        }
    }
}
