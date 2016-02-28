namespace SpotifyTools.Contracts
{
    public interface IProcessAnalyser
    {
        bool IsAppRunning(string processName);
    }
}