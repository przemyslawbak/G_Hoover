using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using G_Hoover.Models;
using System.Text.RegularExpressions;
using G_Hoover.Services.Logging;
using G_Hoover.Services.Config;

namespace G_Hoover.Services.Files
{
    public class FileService : IFileService
    {
        IParamsLogger _log;
        IAppConfig _config;

        private readonly string _logFile;
        private readonly string _resultFile;
        private readonly string _audioFile;

        public FileService(IParamsLogger log, IAppConfig config)
        {
            _log = log;
            _config = config;

            _logFile = _config.LogFile;
            _resultFile = _config.ResultFile;
            _audioFile = _config.AudioFile;
        }

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

        public void RemoveOldLogs()
        {
            _log.Called();

            try
            {
                DeleteFile(_logFile);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

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

        public bool CheckAudioFile()
        {
            _log.Called();

            return (new FileInfo(_audioFile).Length > 10000) ? true : false;
        }

        public string GetAudioFilePath()
        {
            _log.Called();

            return _audioFile;
        }

        public string ProsessText(string audioResult)
        {
            _log.Called(audioResult);

            return Regex.Replace(audioResult.ToLower(), @"[.?!,]", "");
        }

        public void DeleteResultsFile()
        {
            _log.Called();

            DeleteFile(_resultFile);
        }

        public void DeleteOldAudio()
        {
            _log.Called();

            DeleteFile(_audioFile);
        }

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
