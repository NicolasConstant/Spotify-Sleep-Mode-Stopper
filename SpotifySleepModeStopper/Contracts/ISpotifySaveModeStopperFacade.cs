namespace SpotifyTools.Contracts
{
    public interface ISpotifySaveModeStopperFacade
    {
        void ChangeScreenSleep(bool screenRemainsEnabled);
        void ChangeAutoStart(bool autoStartEnabled);
        bool IsScreenSleepEnabled();
        bool IsAutoStartEnabled();
        void StartListening();
        void StopListening();
    }
}