namespace SpotifyTools.Contracts
{
    public interface ISpotifySaveModeStopperFacade
    {
        void ChangeScreenSleep(bool screenRemainsEnabled);
        void ChangeAutoStart(bool autoStartEnabled);
        void ChangeMediaSessionFallback(bool mediaSessionFallbackEnabled);
        bool IsScreenSleepEnabled();
        bool IsAutoStartEnabled();
        bool IsMediaSessionFallbackEnabled();
        void StartListening();
        void StopListening();
    }
}