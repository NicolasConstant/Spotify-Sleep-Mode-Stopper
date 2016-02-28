using System;
using System.IO;
using SpotifyTools.Contracts;
using SpotifyTools.Tools.Model;

namespace SpotifyTools.Tools
{
    public class SettingsManager : ISettingsManager
    {
        private readonly string _fullPathSettingsFile;

        #region Ctor
        public SettingsManager(string appName, string fileName = "settings.config")
        {
            var userAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + appName;

            if (!Directory.Exists(userAppDataFolder)) Directory.CreateDirectory(userAppDataFolder);

            _fullPathSettingsFile = userAppDataFolder + @"\" + fileName;
        }
        #endregion

        public AppSettings GetConfig()
        {
            string configFile;

            if (!File.Exists(_fullPathSettingsFile))
                configFile = ResetConfigFile();
            else
                try
                {
                    configFile = File.ReadAllText(_fullPathSettingsFile);
                    return JsonSerializerHelper.Deserialize<AppSettings>(configFile);
                }
                catch (Exception)
                {
                    configFile = ResetConfigFile();
                }

            return JsonSerializerHelper.Deserialize<AppSettings>(configFile);
        }

        public void SaveConfig(AppSettings settings)
        {
            var serializedConfig = JsonSerializerHelper.Serialize(settings);

            File.WriteAllText(_fullPathSettingsFile, serializedConfig);
        }

        private string ResetConfigFile()
        {
            var newSettings = new AppSettings() { IsScreenSleepEnabled = false };

            SaveConfig(newSettings);
            return JsonSerializerHelper.Serialize(newSettings);
        }
    }
}
