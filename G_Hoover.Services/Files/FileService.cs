using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using G_Hoover.Services.Messages;
using System.Text;
using G_Hoover.Models;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using System.Text.RegularExpressions;
using G_Hoover.Services.Logging;

namespace G_Hoover.Services.Files
{
    public class FileService : IFileService
    {
        ILogService _log;

        private readonly string _logFile = "../../../../log.txt";
        private readonly string _resultFile = "../../../../result.txt";
        private readonly string _audioFile = "../../../../dump.wav";

        public FileService(ILogService log)
        {
            _log = log;
        }

        public async Task<List<string>> GetNewListFromFileAsync(string filePath)
        {
            _log.Called(filePath);

            List<string> list = new List<string>();

            try
            {
                using (var reader = File.OpenText(filePath))
                {
                    var fileText = await reader.ReadToEndAsync();
                    string[] array = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    foreach (string line in array)
                    {
                        list.Add(line);
                    }
                }

                return list;
            }
            catch (Exception e)
            {

                list.Clear();

                _log.Error(e.Message);

                return list;
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
    }
}
