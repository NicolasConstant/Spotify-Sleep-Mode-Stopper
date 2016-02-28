using SpotifyTools.Tools.Model;

namespace SpotifyTools.Contracts
{
    public interface ISettingsManager<T> where T : class
    {
        T GetConfig();
        void SaveConfig(T settings);
    }
}