using System.Configuration;
using System;

namespace G_Hoover.Services.Config
{
    public class AppConfig : IAppConfig
    {
        public string AudioApiKey { get; } = ConfigurationManager.AppSettings["audioApiKey"];
        public string AudioApiRegion { get; } = ConfigurationManager.AppSettings["audioApiRegion"];

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
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["searchPhrase"].Value = searchPhrase;
            config.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public void SavePhraseNo(int phraseNo)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["phraseNo"].Value = phraseNo.ToString();
            config.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection("appSettings");
        }

        public void SaveFilePath(string filePath)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["filePath"].Value = filePath;
            config.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
