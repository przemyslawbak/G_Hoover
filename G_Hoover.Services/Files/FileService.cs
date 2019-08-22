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

namespace G_Hoover.Services.Files
{
    public class FileService : IFileService
    {
        private Logger _logger;
        private IMessageService _messageService;
        private readonly string _logFile = "../../../../log.txt";
        private readonly string _resultFile = "../../../../result.txt";
        private readonly string _audioFile = "../../../../dump.wav";

        public FileService(IMessageService messageService)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _messageService = messageService;

            LoadDictionaries();
        }

        private void LoadDictionaries()
        {
            MessageDictionariesModel messages = _messageService.LoadDictionaries();

            MessagesInfo = messages.MessagesInfo;
            MessagesError = messages.MessagesError;
            MessagesResult = messages.MessagesResult;
            MessagesDisplay = messages.MessagesDisplay;
        }

        public Dictionary<string, string> MessagesInfo { get; set; }
        public Dictionary<string, string> MessagesError { get; set; }
        public Dictionary<string, string> MessagesResult { get; set; }
        public Dictionary<string, string> MessagesDisplay { get; set; }

        public async Task<List<string>> GetNewListFromFileAsync(string filePath)
        {
            List<string> list = new List<string>();
            string callerName = nameof(GetNewListFromFileAsync);

            _logger.Info(MessagesInfo[callerName] + filePath); //log

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

                _logger.Info(MessagesResult[callerName] + list.Count); //log

                return list;
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log

                list.Clear();

                return list;
            }
        }

        public void RemoveOldLogs()
        {
            string callerName = nameof(RemoveOldLogs);

            _logger.Info(MessagesInfo[callerName]); //log

            try
            {
                if (File.Exists(_logFile))
                {
                    File.Delete(_logFile);
                }

                _logger.Info(MessagesResult[callerName]); //log
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
        }

        public void OnDictionariesLoaded(MessageDictionariesModel obj)
        {
            MessagesInfo = obj.MessagesInfo;
            MessagesError = obj.MessagesError;
            MessagesResult = obj.MessagesResult;
        }

        public async Task SaveNewResultAsync(ResultObjectModel result, string phrase)
        {
            string stringResult = CombineStringResult(result, phrase);

            using (TextWriter csvLineBuilder = new StreamWriter(_resultFile, true))
            {
                await csvLineBuilder.WriteLineAsync(stringResult);
            }
        }

        public string CombineStringResult(ResultObjectModel result, string phrase)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(phrase);
            sb.Append("|");
            sb.Append(result.Header);
            sb.Append("|");
            sb.Append(result.Url);

            return sb.ToString();
        }

        public WaveWriter CreateNewWaveWriter(WasapiCapture capture)
        {
            if (File.Exists(_audioFile))
            {
                File.Delete(_audioFile);
            }

            WaveWriter writer = new WaveWriter(_audioFile, capture.WaveFormat);

            return writer;
        }

        public bool CheckAudioFile()
        {
            if (new FileInfo(_audioFile).Length > 10000)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetAudioFilePath()
        {
            return _audioFile;
        }

        public string ProsessText(string audioResult)
        {
            return Regex.Replace(audioResult.ToLower(), @"[.?!,]", "");
        }
    }
}
