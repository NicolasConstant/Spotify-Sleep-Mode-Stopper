using System;
using System.IO;
using SpotifyTools.Contracts;
using SpotifyTools.Tools.Model;

namespace SpotifyTools.Tools
{
    public class SettingsManager<T> : ISettingsManager<T> where T : class
    {
        private readonly string _fullPathSettingsFile;
        private readonly T _defaultValue;

        #region Ctor
        public SettingsManager(string appName, T defaultValue, string fileName = "settings.config")
        {
            var userAppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + appName;

            if (!Directory.Exists(userAppDataFolder)) Directory.CreateDirectory(userAppDataFolder);

            _fullPathSettingsFile = userAppDataFolder + @"\" + fileName;
            _defaultValue = defaultValue;
        }
        #endregion

        public T GetConfig()
        {
            string configFile;

            if (!File.Exists(_fullPathSettingsFile))
                configFile = ResetConfigFile();
            else
                try
                {
                    configFile = File.ReadAllText(_fullPathSettingsFile);
                    var settings = JsonSerializerHelper.Deserialize<T>(configFile);

                    if (((dynamic) settings).Version == ((dynamic) _defaultValue).Version)
                        return settings;

                    configFile = ResetConfigFile();
                }
                catch (Exception e)
                {
                    configFile = ResetConfigFile();
                }

            return JsonSerializerHelper.Deserialize<T>(configFile);
        }

        public void SaveConfig(T settings)
        {
            var serializedConfig = JsonSerializerHelper.Serialize(settings);

            File.WriteAllText(_fullPathSettingsFile, serializedConfig);
        }

        private string ResetConfigFile()
        {
            SaveConfig(_defaultValue);
            return JsonSerializerHelper.Serialize(_defaultValue);
        }
    }
}
