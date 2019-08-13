using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog;
using G_Hoover.Services.Messages;

namespace G_Hoover.Services.Files
{
    public class FileService : IFileService
    {
        private Logger _logger;
        private IMessageService _messageService;
        private readonly string _logFile = "../../../../log.txt";

        public FileService(IMessageService messageService)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _messageService = messageService;
        }
        public Dictionary<string, string> MessagesInfo { get; set; }
        public Dictionary<string, string> MessagesError { get; set; }
        public Dictionary<string, string> MessagesResult { get; set; }
        public string CallerName { get; set; }

        public void LoadDictionaries()
        {
            MessagesInfo = new Dictionary<string, string>();
            MessagesError = new Dictionary<string, string>();
            MessagesResult = new Dictionary<string, string>();

            MessagesInfo = _messageService.GetMessagesInfo();
            MessagesError = _messageService.GetMessagesError();
            MessagesResult = _messageService.GetMessagesResult();
        }

        public async Task<List<string>> GetNewListFromFileAsync(string filePath)
        {
            LoadDictionaries();
            CallerName = _messageService.GetCallerName();
            List<string> list = new List<string>();

            _logger.Info(MessagesInfo[CallerName] + CallerName); //log

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

                _logger.Info(MessagesResult[CallerName] + CallerName); //log

                return list;
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[CallerName] + e.Message + CallerName); //log

                list.Clear();

                return list;
            }
        }

        public void RemoveOldLogs()
        {
            LoadDictionaries();
            CallerName = _messageService.GetCallerName();

            _logger.Info(MessagesInfo[CallerName] + CallerName); //log

            try
            {
                if (File.Exists(_logFile))
                {
                    File.Delete(_logFile);
                }

                _logger.Info(MessagesResult[CallerName] + CallerName); //log
            }
            catch (Exception e)
            {
                _logger.Error(e, MessagesError[CallerName] + CallerName); //log
            }
        }
    }
}
