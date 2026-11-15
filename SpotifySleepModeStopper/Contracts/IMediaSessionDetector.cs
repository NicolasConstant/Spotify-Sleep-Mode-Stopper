namespace SpotifyTools.Contracts
{
    public interface IMediaSessionDetector
    {
        /// <summary>
        /// Returns true if Windows reports any media session as currently playing.
        /// This relies on the System Media Transport Controls (SMTC) API and will
        /// match any player that integrates with Windows media keys (Spotify,
        /// Spotifast, browsers, etc.).
        /// </summary>
        bool IsMediaPlaying();
    }
}
