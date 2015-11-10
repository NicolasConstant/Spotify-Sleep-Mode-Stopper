using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SpotifyTools.Contracts;

namespace SpotifySleepModeStopperGui
{
    public class IconChanger : IMessageDisplayer
    {
        private readonly Action _notifyIsPlaying;
        private readonly Action _notifyIsNotPlaying;

        public IconChanger(Action notifyIsPlaying, Action notifyIsNotPlaying)
        {
            _notifyIsPlaying = notifyIsPlaying;
            _notifyIsNotPlaying = notifyIsNotPlaying;
        }

        public void OutputMessage(string mess)
        {
            if (mess == "Spotify Playing: True")
            {
                _notifyIsPlaying();
            }
            else
            {
                _notifyIsNotPlaying();
            }
        }
    }
}
