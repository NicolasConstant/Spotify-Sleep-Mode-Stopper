using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyTools.Contracts;

namespace SpotifyTools.Domain.AppStatesManagement
{
    public class AppStateReporting : IAppStatusReporting
    {
        private readonly Action _whenAntiSleepModeActivated;
        private readonly Action _whenAntiSleepModeDisabled;

        public AppStateReporting(Action whenAntiSleepModeActivated, Action whenAntiSleepModeDisabled)
        {
            _whenAntiSleepModeActivated = whenAntiSleepModeActivated;
            _whenAntiSleepModeDisabled = whenAntiSleepModeDisabled;
        }

        public void NotifyAntiSleepingModeIsActivated()
        {
            _whenAntiSleepModeActivated();
        }

        public void NotifyAntiSleepingModeIsDisabled()
        {
            _whenAntiSleepModeDisabled();
        }
    }
}
