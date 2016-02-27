using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;
using Microsoft.Win32;
using SpotifyTools;
using SpotifyTools.DomainLayer.AppStatesManagement;
using SpotifyTools.DomainLayer.AudioManagement;
using SpotifyTools.DomainLayer.MessageManagement;
using SpotifyTools.DomainLayer.PowerManagement;
using SpotifyTools.Tools;

namespace SpotifySleepModeStopperGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly Icon _notPlayingIcon;
        private readonly Icon _playingIcon;
        private readonly SpotifySaveModeStopperFacade _facade;

        private const string AppStartingOnStartupMess = "Auto-Start: On";
        private const string AppNotStartingOnStartupMess = "Auto-Start: Off";
        private const string ScreenSleepEnabledMess = "Screen Sleep: On";
        private const string ScreenSleepDisabledMess = "Screen Sleep: Off";
        
        private readonly MenuItem _autoStartMenuItem;
        private readonly MenuItem _screenBehaviorMenuItem;

        //private string _mess;

        public MainWindow()
        {
            InitializeComponent();

            #region Poor Man DI
            var iconChanger = new AppStateReporting(SetPlayingGui, SetNotPlayingGui);
            var messageDisplayer = new DummyMessageDisplayer();
            var powerHandler = new PowerRequestContextHandler();
            var soundAnalyser = new CsCoreSoundAnalyser(messageDisplayer);

            var fullPath = Assembly.GetExecutingAssembly().Location;
            var autoStartManager = new AutoStartManager("SpotifySleepModeStopper", fullPath);

            var settingsManager = new SettingsManager();
            #endregion

            _facade = new SpotifySaveModeStopperFacade(messageDisplayer, powerHandler, soundAnalyser, iconChanger, autoStartManager, settingsManager);

            #region Events Subscription
            Closing += OnClosing;
            #endregion

            #region Init Tray Icon
            WindowState = WindowState.Minimized;
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;

            _notifyIcon = new NotifyIcon();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpotifySleepModeStopperGui.music.ico"))
            {
                _notPlayingIcon = new Icon(stream);
            }
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpotifySleepModeStopperGui.music_playing.ico"))
            {
                _playingIcon = new Icon(stream);
            }
            SetNotPlayingGui();
            _notifyIcon.Visible = true;

            var contextMenu = new ContextMenu();
            var exitMenuItem = new MenuItem();
            exitMenuItem.Index = 2;
            exitMenuItem.Text = "Exit";
            exitMenuItem.Click += exit_Click;

            _screenBehaviorMenuItem = new MenuItem();
            _screenBehaviorMenuItem.Index = 1;

            if (_facade.IsScreenSleepEnabled())
                _screenBehaviorMenuItem.Text = ScreenSleepEnabledMess;
            else
                _screenBehaviorMenuItem.Text = ScreenSleepDisabledMess;

            _screenBehaviorMenuItem.Click += ScreenBehaviorMenuItemOnClick;

            _autoStartMenuItem = new MenuItem();
            _autoStartMenuItem.Index = 0;

            if (_facade.IsAutoStartEnabled())
                _autoStartMenuItem.Text = AppStartingOnStartupMess; //"Auto Startup";
            else
                _autoStartMenuItem.Text = AppNotStartingOnStartupMess;

            _autoStartMenuItem.Click += AutoStartMenuItemOnClick;

            contextMenu.MenuItems.AddRange(new[] { _autoStartMenuItem, _screenBehaviorMenuItem, exitMenuItem });
            _notifyIcon.ContextMenu = contextMenu;
            #endregion
            
            _facade.StartListening();
        }

        private void ScreenBehaviorMenuItemOnClick(object sender, EventArgs eventArgs)
        {
            var isSet = _facade.IsScreenSleepEnabled();

            //Change Value 
            _facade.ChangeScreenSleep(!isSet);

            //Update GUI
            if (!isSet)
                _screenBehaviorMenuItem.Text = ScreenSleepEnabledMess;
            else
                _screenBehaviorMenuItem.Text = ScreenSleepDisabledMess;
        }

        private void AutoStartMenuItemOnClick(object sender, EventArgs e)
        {
            var isSet = _facade.IsAutoStartEnabled();

            //Change Value
            _facade.ChangeAutoStart(!isSet);

            //Update GUI
            if (!isSet)
                _autoStartMenuItem.Text = AppStartingOnStartupMess;
            else
                _autoStartMenuItem.Text = AppNotStartingOnStartupMess;
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _facade.StopListening();

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            _screenBehaviorMenuItem.Click -= ScreenBehaviorMenuItemOnClick;
            _autoStartMenuItem.Click -= AutoStartMenuItemOnClick;
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SetNotPlayingGui()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                _notifyIcon.Icon = _notPlayingIcon;
                _notifyIcon.Text = "Spotify isn't playing";
            }));
        }

        private void SetPlayingGui()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                _notifyIcon.Icon = _playingIcon;
                _notifyIcon.Text = "Spotify is playing";
            }));
        }
    }
}
