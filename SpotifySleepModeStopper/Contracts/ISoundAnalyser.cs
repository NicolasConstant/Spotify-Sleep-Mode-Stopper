namespace SpotifyTools.Contracts
{
    public interface ISoundAnalyser
    {
        bool IsWindowsOutputingSound();
        bool IsProcessNameOutputingSound(string processName);
    }
}