namespace SpotifyTools.Contracts
{
    public interface IAppStatusReporting
    {
        void NotifyAntiSleepingModeIsActivated();
        void NotifyAntiSleepingModeIsDisabled();
    }
}