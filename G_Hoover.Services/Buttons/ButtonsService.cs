using CefSharp.Wpf;
using G_Hoover.Models;
using G_Hoover.Services.Browsing;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Services.Buttons
{
    public class ButtonsService : IButtonsService
    {
        private readonly Logger _logger;
        private readonly IMessageService _messageService;
        private readonly IControlsService _controlsService;
        private readonly IFileService _fileService;
        private readonly IBrowsingService _browserService;

        public ButtonsService(IMessageService messageService,
            IControlsService controlService,
            IFileService fileService,
            IBrowsingService browserService)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _controlsService = controlService;
            _fileService = fileService;
            _messageService = messageService;
            _browserService = browserService;

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

        public void ExecuteStopButton()
        {
            string callerName = nameof(ExecuteStopButton);

            _logger.Info(MessagesInfo[callerName]); //log

            try
            {
                _browserService.CancelCollectData(); //cancel

                _logger.Info(MessagesResult[callerName]); //log
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
            finally
            {
                _controlsService.GetStoppedConfiguration(); //ui
            }
        }

        public void ExecutePauseButton(bool paused)
        {
            string callerName = nameof(ExecutePauseButton);

            _logger.Info(MessagesInfo[callerName] + paused); //log

            try
            {
                if (paused == true)
                {
                    _controlsService.GetStartedConfiguration(); //ui
                }
                else
                {
                    _controlsService.GetPausedConfiguration(); //ui
                }

                _logger.Info(MessagesResult[callerName] + paused); //log
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
        }

        public async Task ExecuteStartButtonAsync(List<string> nameList, IWpfWebBrowser webBrowser, string searchPhrase)
        {
            string callerName = nameof(ExecuteStartButtonAsync);

            _logger.Info(MessagesInfo[callerName] + nameList.Count + ";" + searchPhrase); //log

            try
            {
                await _browserService.CollectDataAsync(nameList, webBrowser, searchPhrase); //browser service

                _logger.Info(MessagesResult[callerName]); //log


            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
            finally
            {
                _controlsService.GetStoppedConfiguration(); //ui
            }
        }

        public async Task<List<string>> ExecuteUploadButtonAsync(string filePath)
        {
            List<string> nameList = new List<string>();
            string callerName = nameof(ExecuteUploadButtonAsync);

            _logger.Info(MessagesInfo[callerName] + filePath); //log

            try
            {

                if (!string.IsNullOrEmpty(filePath))
                {
                    _controlsService.GetWaitConfiguration(); //ui
                    nameList = await _fileService.GetNewListFromFileAsync(filePath); //file
                    _controlsService.GetStoppedConfiguration(); //ui

                    if (nameList.Count > 0)
                    {
                        _browserService.CancelCollectData(); //cancel

                        _logger.Info(MessagesResult[callerName] + nameList.Count); //log

                        return nameList;
                    }
                    else
                    {
                        throw new Exception("Empty file.");
                    }
                }
                else
                {
                    throw new Exception("Cancelled.");
                }
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log

                return new List<string>();
            }
        }

        public async Task ExecuteBuildButtonAsync(string searchPhrase)
        {
            string callerName = nameof(ExecuteBuildButtonAsync);

            _logger.Info(MessagesInfo[callerName]); //log

            try
            {


                if (!string.IsNullOrEmpty(searchPhrase))
                {
                    await _fileService.SavePhraseAsync(searchPhrase); //file

                    _browserService.UpdateSearchPhrase(searchPhrase);

                    _logger.Info(MessagesResult[callerName] + searchPhrase); //log
                }
                else
                {
                    throw new Exception("Incorrect phrase.");
                }
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
        }
    }
}
