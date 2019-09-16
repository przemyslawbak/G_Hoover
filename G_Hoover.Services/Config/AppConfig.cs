using System.Configuration;

namespace G_Hoover.Services.Config
{
    /// <summary>
    /// class that reads and saves app configuration
    /// </summary>
    public class AppConfig : IAppConfig
    {
        public string AudioApiKey { get; } = ConfigurationManager.AppSettings["audioApiKey"]; //loads API key from config file
        public string AudioApiRegion { get; } = ConfigurationManager.AppSettings["audioApiRegion"]; //loads API region from config file
        public string LogFile { get; } = ConfigurationManager.AppSettings["logFile"]; //loads log file path from config file
        public string ResultFile { get; } = ConfigurationManager.AppSettings["resultFile"]; //loads results file path from config file
        public string AudioFile { get; } = ConfigurationManager.AppSettings["audioFile"]; //loads audio file path from config file

        /// <summary>
        /// gets file path with names/phrases from assembly config
        /// </summary>
        /// <returns>string path</returns>
        public string GetFilePath()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string toReturn = config.AppSettings.Settings["filePath"].Value;

            return toReturn;
        }

        /// <summary>
        /// gets phrase No from assembly config
        /// </summary>
        /// <returns>int number</returns>
        public int GetPhraseNo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            int toReturn = int.Parse(config.AppSettings.Settings["phraseNo"].Value);

            return toReturn;
        }

        /// <summary>
        /// gets built serach phrase template from assembly config
        /// </summary>
        /// <returns>string phrase</returns>
        public string GetSearchPhrase()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string toReturn = config.AppSettings.Settings["searchPhrase"].Value;

            return toReturn;
        }

        /// <summary>
        /// saves built serach phrase in assembly config
        /// </summary>
        /// <param name="searchPhrase">string phrase</param>
        public void SaveSearchPhrase(string searchPhrase)
        {
            SaveInSettings(searchPhrase, nameof(searchPhrase));
        }

        /// <summary>
        /// saves phrase number in assembly config
        /// </summary>
        /// <param name="phraseNo">int number</param>
        public void SavePhraseNo(int phraseNo)
        {
            SaveInSettings(phraseNo.ToString(), nameof(phraseNo));
        }

        /// <summary>
        /// saves file path with names/phrases in assembly config
        /// </summary>
        /// <param name="filePath">string path</param>
        public void SaveFilePath(string filePath)
        {
            SaveInSettings(filePath, nameof(filePath));
        }

        /// <summary>
        /// DRY saving configuration properties
        /// </summary>
        /// <param name="value">prop value</param>
        /// <param name="nameOf">prop name</param>
        private void SaveInSettings(string value, string nameOf)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[nameOf].Value = value;
            config.Save(ConfigurationSaveMode.Minimal);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
