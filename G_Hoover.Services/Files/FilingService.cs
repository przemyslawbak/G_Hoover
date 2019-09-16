using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using G_Hoover.Models;
using System.Text.RegularExpressions;
using G_Hoover.Services.Config;
using Params_Logger;

namespace G_Hoover.Services.Files
{
    /// <summary>
    /// service class that operates on files and folders
    /// </summary>
    public class FilingService : IFilingService
    {
        private readonly IAppConfig _config;
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();

        private readonly string _logFile;
        private readonly string _resultFile;
        private readonly string _audioFile;

        public FilingService(IAppConfig config)
        {
            _config = config;

            _resultFile = _config.ResultFile;
            _audioFile = _config.AudioFile;
        }

        /// <summary>
        /// gets list of phrases/names from specified file
        /// </summary>
        /// <param name="filePath">file path</param>
        /// <returns>list of phrases/names</returns>
        public async Task<List<string>> GetNewListFromFileAsync(string filePath)
        {
            _log.Called(filePath);

            try
            {
                List<string> list = new List<string>();

                string[] array = await GetArrayAsync(filePath);

                foreach (string line in array)
                {
                    list.Add(line);
                }

                return list;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);

                return new List<string>();
            }
        }

        /// <summary>
        /// saving new resut object line
        /// </summary>
        /// <param name="result">object</param>
        /// <param name="phrase">phrase name for this object</param>
        /// <returns></returns>
        public async Task SaveNewResultAsync(ResultObjectModel result, string phrase)
        {
            _log.Called(string.Empty, phrase);

            try
            {
                string stringResult = CombineStringResult(result, phrase);

                if (!string.IsNullOrEmpty(stringResult))
                {
                    using (TextWriter LineBuilder = new StreamWriter(_resultFile, true))
                    {
                        await LineBuilder.WriteLineAsync(stringResult);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// combines string to be saved in result file
        /// </summary>
        /// <param name="result">result</param>
        /// <param name="phrase">phrase name for this object</param>
        /// <returns></returns>
        public string CombineStringResult(ResultObjectModel result, string phrase)
        {
            _log.Called(string.Empty, phrase);

            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(phrase);
                sb.Append("|");
                sb.Append(result.Header);
                sb.Append("|");
                sb.Append(result.Url);

                return sb.ToString();
            }
            catch (Exception e)
            {
                _log.Error(e.Message);

                return string.Empty;
            }
        }

        /// <summary>
        /// checks if audio file exists
        /// </summary>
        /// <returns>bool exists or not</returns>
        public bool CheckAudioFile()
        {
            _log.Called();

            return (new FileInfo(_audioFile).Length > 10000) ? true : false;
        }

        /// <summary>
        /// returns audio file path
        /// </summary>
        /// <returns>audio file path</returns>
        public string GetAudioFilePath()
        {
            _log.Called();

            return _audioFile;
        }

        /// <summary>
        /// cleaning up with Regex result srting
        /// </summary>
        /// <param name="audioResult">audio text result</param>
        /// <returns>string text after clean up</returns>
        public string ProsessText(string audioResult)
        {
            _log.Called(audioResult);

            return Regex.Replace(audioResult.ToLower(), @"[.?!,]", "");
        }

        /// <summary>
        /// deletes result file
        /// </summary>
        public void DeleteResultsFile()
        {
            _log.Called();

            DeleteFile(_resultFile);
        }

        /// <summary>
        /// deletes audio file
        /// </summary>
        public void DeleteOldAudio()
        {
            _log.Called();

            DeleteFile(_audioFile);
        }

        /// <summary>
        /// DRY delete file
        /// </summary>
        /// <param name="path">file path</param>
        private void DeleteFile(string path)
        {
            _log.Called(path);

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// returns number of lines (records) in result file
        /// </summary>
        /// <returns>int number</returns>
        public async Task<int> GetHowManyRecordsAsync()
        {
            _log.Called();

            try
            {
                int counter = 0;

                string[] array = await GetArrayAsync(_resultFile);

                foreach (string line in array)
                {
                    if (!string.IsNullOrEmpty(line))
                        counter++;
                }

                return counter;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);

                return 0;
            }
        }

        /// <summary>
        /// DRY for reading string array
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task<string[]> GetArrayAsync(string path)
        {
            using (var reader = File.OpenText(path))
            {
                var fileText = await reader.ReadToEndAsync();

                return fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            }
        }
    }
}
