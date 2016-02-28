using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpotifyTools.Tools;
using SpotifyTools.Tools.Model;

namespace UnitTestProject
{
    [TestClass]
    public class SettingsUnitTest
    {
        private const string AppName = "SpotifySleepModeStopperTesting";
        private readonly string _appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\" + AppName;

        [TestMethod]
        public void FirstSettingsDeployementTestMethod()
        {
            const string configName = "settings.config";
            var fullFileName = _appDataFolder + @"\" + configName;
            if (!Directory.Exists(_appDataFolder)) Directory.CreateDirectory(_appDataFolder);
            if (File.Exists(fullFileName)) File.Delete(fullFileName);

            Assert.IsFalse(File.Exists(fullFileName));

            var settingsManager = new SettingsManager(AppName);
            settingsManager.GetConfig(); //First get => Reset config

            Assert.IsTrue(File.Exists(fullFileName));
        }

        [TestMethod]
        public void SaveSettingsTestMethod()
        {
            const string configName = "save_settings.config";
            const string configContentTrue = "\"IsScreenSleepEnabled\":true";
            const string configContentFalse = "\"IsScreenSleepEnabled\":false";

            var fullFileName = _appDataFolder + @"\" + configName;
            if (!Directory.Exists(_appDataFolder)) Directory.CreateDirectory(_appDataFolder);

            var appSettings = new AppSettings() {IsScreenSleepEnabled = true};

            var settingsManager = new SettingsManager(AppName, configName);
            settingsManager.SaveConfig(appSettings);

            Assert.IsTrue(File.Exists(fullFileName));

            var fileContent = File.ReadAllText(fullFileName);
            Assert.IsTrue(fileContent.Contains(configContentTrue));
            Assert.IsFalse(fileContent.Contains(configContentFalse));

            appSettings.IsScreenSleepEnabled = false;
            settingsManager.SaveConfig(appSettings);
            
            fileContent = File.ReadAllText(fullFileName);
            Assert.IsTrue(fileContent.Contains(configContentFalse));
            Assert.IsFalse(fileContent.Contains(configContentTrue));
        }

        [TestMethod]
        public void LoadSettingsTestMethod()
        {
            const string configName = "load_settings.config";
            const string configContentFalse = "{\"IsScreenSleepEnabled\":false}";
            const string configContentTrue = "{\"IsScreenSleepEnabled\":true}";

            var fullFileName = _appDataFolder + @"\" + configName;
            if (!Directory.Exists(_appDataFolder)) Directory.CreateDirectory(_appDataFolder);
            if (File.Exists(fullFileName)) File.Delete(fullFileName);

            File.WriteAllText(fullFileName, configContentFalse);

            var settingsManager = new SettingsManager(AppName, configName);
            var config = settingsManager.GetConfig();

            Assert.IsFalse(config.IsScreenSleepEnabled);

            File.WriteAllText(fullFileName, configContentTrue);

            config = settingsManager.GetConfig();

            Assert.IsTrue(config.IsScreenSleepEnabled);
        }

        [TestMethod]
        public void LoadCorruptSettingsTestMethod()
        {
            const string configName = "corrupt_settings.config";
            const string corruptConfigContent = "\"IsScredsqdsqs\"enSleepEnabled\":false}";
            
            var fullFileName = _appDataFolder + @"\" + configName;
            if (!Directory.Exists(_appDataFolder)) Directory.CreateDirectory(_appDataFolder);
            if (File.Exists(fullFileName)) File.Delete(fullFileName);

            File.WriteAllText(fullFileName, corruptConfigContent);

            var settingsManager = new SettingsManager(AppName, configName);
            var config = settingsManager.GetConfig();

            Assert.IsFalse(config.IsScreenSleepEnabled);
        }
    }
}
