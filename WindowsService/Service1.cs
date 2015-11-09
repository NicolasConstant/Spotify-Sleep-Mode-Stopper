using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using SpotifyTools.Domain;
using SpotifyTools.Tools;

namespace PreventSpotifyInterruptionService
{
    public partial class Service1 : ServiceBase
    {
        private readonly SpotifySaveModeStopper _spotifyAnalyser;

        public Service1()
        {
            InitializeComponent();
            _spotifyAnalyser = new SpotifySaveModeStopper(new DummyMessageDisplayer(), new PreventSleepScreen(), new NAudioSoundAnalyser());
        }

        protected override void OnStart(string[] args)
        {
            _spotifyAnalyser.StartListening();
        }

        protected override void OnStop()
        {
            _spotifyAnalyser.StopListening();
        }
    }
}
