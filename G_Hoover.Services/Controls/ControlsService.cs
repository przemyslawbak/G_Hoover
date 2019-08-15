﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using G_Hoover.Events;
using G_Hoover.Models;
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

        public ControlsService(IMessageService messageService, IEventAggregator eventAggregator)
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
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        public void GetWaitConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = true,
                PleaseWaitVisible = true,
                UiButtonsEnabled = false,
                StopBtnEnabled = false,
                PauseBtnEnabled = false,
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        public void ExecuteStopButton()
        {
            string callerName = nameof(ExecuteStopButton);

            _logger.Info(MessagesInfo[callerName]); //log

            try
            {
                //TokenSource.Cancel();

                _logger.Info(MessagesResult[callerName]); //log
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
            finally
            {
                GetStoppedConfiguration();
            }
        }

        public void ExecutePauseButton(bool paused)
        {
            string callerName = nameof(ExecuteStopButton);

            _logger.Info(MessagesInfo[callerName]); //log

            try
            {
                if (paused == true)
                {
                    GetStartedConfiguration();

                    paused = false;

                    //and browser service here;
                }
                else
                {
                    GetPausedConfiguration();

                    paused = true;

                    //and browser service here;
                }

                _logger.Info(MessagesResult[callerName] + paused); //log
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
        }
    }
}