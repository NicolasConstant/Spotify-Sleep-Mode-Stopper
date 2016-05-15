using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using SpotifyTools.Tools.Model;

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
        private const string ExitMess = "Exit";

        private readonly MenuItem _autoStartMenuItem;
        private readonly MenuItem _screenBehaviorMenuItem;
        private readonly MenuItem _screenLockMenuItem;
        private readonly MenuItem _donationMenuItem;
        private readonly MenuItem _exitMenuItem;

        private const string AppName = "SpotifySleepModeStopper";

        public MainWindow()
        {
            InitializeComponent();

            #region Poor Man DI
            var iconChanger = new AppStateReporting(SetPlayingGui, SetNotPlayingGui);
            var messageDisplayer = new DummyMessageDisplayer();
            var powerHandler = new PowerRequestContextHandler();
            var soundAnalyser = new CsCoreSoundAnalyser(messageDisplayer);
            var processAnalyser = new ProcessAnalyser();

            var fullPath = Assembly.GetExecutingAssembly().Location;
            var autoStartManager = new AutoStartManager(AppName, fullPath);
            
            var defaultSettings = new AppSettings
            {
                IsScreenSleepEnabled = false,
                DonationMessageActive = true,
                ScreenLockActive = false
            };
            var settingsManager = new SettingsManager<AppSettings>(AppName, defaultSettings);
            #endregion

            _facade = new SpotifySaveModeStopperFacade(messageDisplayer, powerHandler, soundAnalyser, processAnalyser, iconChanger, autoStartManager, settingsManager);

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

            //Exit
            _exitMenuItem = new MenuItem();
            _exitMenuItem.Index = 50;
            _exitMenuItem.Text = ExitMess;
            _exitMenuItem.Click += ExitOnClick;

            //Screen Behavior
            _screenBehaviorMenuItem = new MenuItem();
            _screenBehaviorMenuItem.Index = 25;

            if (_facade.IsScreenSleepEnabled())
                _screenBehaviorMenuItem.Text = ScreenSleepEnabledMess;
            else
                _screenBehaviorMenuItem.Text = ScreenSleepDisabledMess;

            _screenBehaviorMenuItem.Click += ScreenBehaviorMenuItemOnClick;

            //LockScreen
            if (_facade.IsLockScreenActive())
            {
                _screenLockMenuItem = new MenuItem();
                _screenLockMenuItem.Index = 20;
                _screenLockMenuItem.Text = "Lock Screen: Off";
                _screenLockMenuItem.Click += LockScreenMenuItemOnClick;
            }

            //Donate
            if (_facade.IsDonationActive())
            {
                _donationMenuItem = new MenuItem();
                _donationMenuItem.Index = 10;
                _donationMenuItem.Text = "Donate?";
                _donationMenuItem.Click += DonationMenuItemOnClick;
            }

            //Autostart
            _autoStartMenuItem = new MenuItem();
            _autoStartMenuItem.Index = 0;

            if (_facade.IsAutoStartEnabled())
                _autoStartMenuItem.Text = AppStartingOnStartupMess;
            else
                _autoStartMenuItem.Text = AppNotStartingOnStartupMess;
            _autoStartMenuItem.Click += AutoStartMenuItemOnClick;

            //Add all menu items
            var menuList = new List<MenuItem>();
            menuList.Add(_autoStartMenuItem);
            menuList.Add(_screenBehaviorMenuItem);
            if (_screenLockMenuItem != null) menuList.Add(_autoStartMenuItem);
            if (_donationMenuItem != null) menuList.Add(_donationMenuItem);
            menuList.Add(_exitMenuItem);

            contextMenu.MenuItems.AddRange(menuList.ToArray());
            _notifyIcon.ContextMenu = contextMenu;
            #endregion

            _facade.StartListening();
        }

        private void LockScreenMenuItemOnClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DonationMenuItemOnClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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

        private void DonateOnClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LockScreenOnClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _facade.StopListening();

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            _screenBehaviorMenuItem.Click -= ScreenBehaviorMenuItemOnClick;
            _autoStartMenuItem.Click -= AutoStartMenuItemOnClick;
            _exitMenuItem.Click -= ExitOnClick;
            if(_donationMenuItem != null) _donationMenuItem.Click -= DonationMenuItemOnClick;
            if(_screenLockMenuItem != null) _screenLockMenuItem.Click += LockScreenMenuItemOnClick;
        }

        private void ExitOnClick(object sender, EventArgs e)
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
