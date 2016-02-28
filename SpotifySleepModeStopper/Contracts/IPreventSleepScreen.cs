namespace SpotifyTools.Contracts
{
    public interface IPreventSleepScreen
    {
        /// <summary>
        /// Prevent screensaver, display dimming and power saving. This function wraps PInvokes on Win32 API. 
        /// </summary>
        /// <param name="enableConstantPower"></param>
        /// <param name="enableConstantDisplay"></param>
        void EnableConstantDisplayAndPower(bool enableConstantPower, bool enableConstantDisplay);
    }
}