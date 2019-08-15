using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using G_Hoover.Services.Messages;
using System.Text;
using G_Hoover.Models;

namespace G_Hoover.Services.Files
{
    public class FileService : IFileService
    {
        private Logger _logger;
        private IMessageService _messageService;
        private readonly string _logFile = "../../../../log.txt";
        private readonly string _phraseFile = "../../../../phrase.txt";

        public FileService(IMessageService messageService)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _messageService = messageService;

            LoadDictionaries();
        }
        private void LoadDictionaries()
        {
            MessageDictionaries messages = _messageService.LoadDictionaries();

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
            string callerName = nameof(GetNewListFromFileAsync);
            List<string> list = new List<string>();

            _logger.Info(MessagesInfo[callerName] + callerName); //log

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

                _logger.Info(MessagesResult[callerName] + callerName); //log

                return list;
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message + callerName); //log

                list.Clear();

                return list;
            }
        }

        public void RemoveOldLogs()
        {
            string callerName = nameof(RemoveOldLogs);

            _logger.Info(MessagesInfo[callerName] + callerName); //log

            try
            {
                if (File.Exists(_logFile))
                {
                    File.Delete(_logFile);
                }

                _logger.Info(MessagesResult[callerName] + callerName); //log
            }
            catch (Exception e)
            {
                _logger.Error(e, MessagesError[callerName] + callerName); //log
            }
        }

        public async Task<string> LoadPhraseAsync()
        {
            string callerName = nameof(LoadPhraseAsync);

            _logger.Info(MessagesInfo[callerName] + callerName); //log

            try
            {
                if (File.Exists(_phraseFile))
                {
                    using (StreamReader reader = File.OpenText(_phraseFile))
                    {
                        string fileText = await reader.ReadToEndAsync();

                        _logger.Info(MessagesResult[callerName] + callerName); //log

                        return fileText;
                    }
                }
                else
                {
                    throw new Exception("File not found.");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, MessagesError[callerName] + callerName); //log

                return string.Empty;
            }
        }

        public async Task SavePhraseAsync(string searchPhrase)
        {
            string callerName = nameof(SavePhraseAsync);

            _logger.Info(MessagesInfo[callerName] + callerName); //log

            try
            {
                if (File.Exists(_phraseFile))
                {
                    File.Delete(_phraseFile);
                }

                using (var writer = File.OpenWrite(_phraseFile))
                {
                    using (var streamWriter = new StreamWriter(writer))
                    {
                        await streamWriter.WriteAsync(searchPhrase);
                    }
                }

                _logger.Info(MessagesResult[callerName] + callerName); //log
            }
            catch (Exception e)
            {
                _logger.Error(e, MessagesError[callerName] + callerName); //log
            }
        }

        private void OnDictionariesLoaded(MessageDictionaries obj)
        {
            MessagesInfo = obj.MessagesInfo;
            MessagesError = obj.MessagesError;
            MessagesResult = obj.MessagesResult;
        }
    }
}
