using System.Collections.Generic;
using System.Windows;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using NLog;
using Prism.Events;

namespace G_Hoover.Services.Controls
{
    public class ControlsService : IControlsService
    {
        private readonly Logger _logger;
        private readonly IMessageService _messageService;
        private readonly IEventAggregator _eventAggregator;

        public ControlsService(IMessageService messageService,
            IEventAggregator eventAggregator,
            IFileService fileService)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _messageService = messageService;
            _eventAggregator = eventAggregator;

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

        public void GetPausedConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = true,
                PleaseWaitVisible = false,
                UiButtonsEnabled = true,
                StopBtnEnabled = true,
                PauseBtnEnabled = true,
                Stopped = false
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        public void GetStartedConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = false,
                PleaseWaitVisible = false,
                UiButtonsEnabled = false,
                StopBtnEnabled = true,
                PauseBtnEnabled = true,
                Stopped = false
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        public void GetStoppedConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = false,
                PleaseWaitVisible = false,
                UiButtonsEnabled = true,
                StopBtnEnabled = false,
                PauseBtnEnabled = false,
                Stopped = true
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        public void GetWaitConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = false,
                PleaseWaitVisible = true,
                UiButtonsEnabled = false,
                StopBtnEnabled = false,
                PauseBtnEnabled = false,
                Stopped = false
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        public void ShowLessBrowser()
        {
            BrowserPropertiesModel browser = new BrowserPropertiesModel()
            {
                WindowState = WindowState.Normal,
                IsBrowserFocused = false,
                IsOnTop = false,
                ResizeMode = ResizeMode.CanResize
            };

            _eventAggregator.GetEvent<UpdateBrowserEvent>().Publish(browser);
        }

        public void ShowMoreBrowserRunning()
        {
            BrowserPropertiesModel browser = new BrowserPropertiesModel()
            {
                WindowState = WindowState.Maximized,
                IsBrowserFocused = true,
                IsOnTop = true,
                ResizeMode = ResizeMode.NoResize
            };

            _eventAggregator.GetEvent<UpdateBrowserEvent>().Publish(browser);
        }

        public void ShowMoreBrowserPaused()
        {
            BrowserPropertiesModel browser = new BrowserPropertiesModel()
            {
                WindowState = WindowState.Maximized,
                IsBrowserFocused = true,
                IsOnTop = false,
                ResizeMode = ResizeMode.CanResize
            };

            _eventAggregator.GetEvent<UpdateBrowserEvent>().Publish(browser);
        }
    }
}
