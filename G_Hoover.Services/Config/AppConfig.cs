using System.Configuration;
using System;

namespace G_Hoover.Services.Config
{
    public class AppConfig : IAppConfig
    {
        public string AudioApiKey { get; } = ConfigurationManager.AppSettings["audioApiKey"];
        public string AudioApiRegion { get; } = ConfigurationManager.AppSettings["audioApiRegion"];
        public string LogFile { get; } = ConfigurationManager.AppSettings["logFile"];
        public string ResultFile { get; } = ConfigurationManager.AppSettings["resultFile"];
        public string AudioFile { get; } = ConfigurationManager.AppSettings["audioFile"];

        public string GetFilePath()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string toReturn = config.AppSettings.Settings["filePath"].Value;

            return toReturn;
        }

        public int GetPhraseNo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            int toReturn = int.Parse(config.AppSettings.Settings["phraseNo"].Value);

            return toReturn;
        }

        public string GetSearchPhrase()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string toReturn = config.AppSettings.Settings["searchPhrase"].Value;

            return toReturn;
        }

        public void SaveSearchPhrase(string searchPhrase)
        {
            SaveInSettings(searchPhrase, nameof(searchPhrase));
        }

        public void SavePhraseNo(int phraseNo)
        {
            SaveInSettings(phraseNo.ToString(), nameof(phraseNo));
        }

        public void SaveFilePath(string filePath)
        {
            SaveInSettings(filePath, nameof(filePath));
        }

        private void SaveInSettings(string item, string nameOf)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[nameOf].Value = item;
            config.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
