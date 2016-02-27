using SpotifyTools.Tools.Model;

namespace SpotifyTools.Contracts
{
    public interface ISettingsManager
    {
        AppSettings GetConfig();
        void SaveConfig(AppSettings settings);
    }
}