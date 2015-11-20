using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using SpotifyTools.Domain;
using SpotifyTools.Domain.AppStatesManagement;
using SpotifyTools.Domain.AudioManagement;
using SpotifyTools.Domain.MessageManagement;
using SpotifyTools.Domain.PowerManagement;
using Application = System.Windows.Application;

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
        private readonly SpotifySaveModeStopper _analyser;

        public MainWindow()
        {
            InitializeComponent();

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
            using ( var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SpotifySleepModeStopperGui.music_playing.ico"))
            {
                _playingIcon = new Icon(stream);
            }
            SetNotPlaying();
            _notifyIcon.Visible = true;

            var contextMenu = new ContextMenu();
            var menuItem = new MenuItem();

            contextMenu.MenuItems.AddRange(new[] { menuItem });
            menuItem.Index = 0;
            menuItem.Text = "Exit";
            menuItem.Click += exit_Click;
            _notifyIcon.ContextMenu = contextMenu;
            #endregion
            
            #region Poor Man DI
            var iconChanger = new AppStateReporting(SetPlaying, SetNotPlaying);
            var messageDisplayer = new DummyMessageDisplayer();
            var powerHandler = new PowerRequestContextHandler();
            var soundAnalyser = new CsCoreSoundAnalyser(messageDisplayer);
            #endregion

            _analyser = new SpotifySaveModeStopper(messageDisplayer, powerHandler, soundAnalyser, iconChanger);
            _analyser.StartListening();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            _analyser.StopListening();

            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            //Environment.Exit(0);
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SetNotPlaying()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                _notifyIcon.Icon = _notPlayingIcon;
                _notifyIcon.Text = "Spotify isn't playing";
            }));
        }

        private void SetPlaying()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                _notifyIcon.Icon = _playingIcon;
                _notifyIcon.Text = "Spotify is playing";
            }));
        }
    }
}
