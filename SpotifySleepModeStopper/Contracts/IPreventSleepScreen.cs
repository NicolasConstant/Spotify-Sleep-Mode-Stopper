namespace SpotifyTools.Contracts
{
    public interface IPreventSleepScreen
    {
        /// <summary>
        /// Prevent screensaver, display dimming and power saving. This function wraps PInvokes on Win32 API. 
        /// </summary>
        /// <param name="enableConstantDisplayAndPower">True to get a constant display and power - False to clear the settings</param>
        void EnableConstantDisplayAndPower(bool enableConstantDisplayAndPower);
    }
}