using System;
using SpotifyTools.Contracts;

namespace SpotifyTools.DomainLayer.AppStatesManagement
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
