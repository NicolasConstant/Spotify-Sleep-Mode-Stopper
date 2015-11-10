using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpotifyTools.Domain;
using SpotifyTools.Domain.AudioManagement;
using SpotifyTools.Domain.MessageManagement;
using SpotifyTools.Domain.PowerManagement;

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

            #region Init Tray Icon
            WindowState = WindowState.Minimized;
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;

            _notifyIcon = new NotifyIcon();
            using (
                var stream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream("SpotifySleepModeStopperGui.music.ico"))
            {
                _notPlayingIcon = new Icon(stream);
            }
            using (
                var stream =
                    Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("SpotifySleepModeStopperGui.music_playing.ico"))
            {
                _playingIcon = new Icon(stream);
            }
            SetNotPlaying();
            _notifyIcon.Visible = true;

            var contextMenu = new System.Windows.Forms.ContextMenu();
            var menuItem = new System.Windows.Forms.MenuItem();

            contextMenu.MenuItems.AddRange(new[] { menuItem });
            menuItem.Index = 0;
            menuItem.Text = "Exit";
            menuItem.Click += exit_Click;
            _notifyIcon.ContextMenu = contextMenu;
            #endregion

            var iconChanger = new IconChanger(SetPlaying, SetNotPlaying);
            _analyser = new SpotifySaveModeStopper(iconChanger, new PowerRequestContextHandler(), new CsCoreSoundAnalyser());
            _analyser.StartListening();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            _analyser.StopListening();
            Environment.Exit(0);
        }

        private void SetNotPlaying()
        {
            _notifyIcon.Icon = _notPlayingIcon;
            _notifyIcon.Text = "Spotify isn't playing";
        }

        private void SetPlaying()
        {
            _notifyIcon.Icon = _playingIcon;
            _notifyIcon.Text = "Spotify is playing";
        }
    }
}
