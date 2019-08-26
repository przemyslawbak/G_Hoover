﻿using CefSharp.Wpf;
using G_Hoover.Services.Browsing;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using G_Hoover.Services.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G_Hoover.Services.Buttons
{
    public class ButtonsService : IButtonsService
    {
        private readonly IControlsService _controlsService;
        private readonly IFileService _fileService;
        private readonly IBrowseService _browseService;
        private readonly ILogService _log;

        public ButtonsService(IControlsService controlService,
            IFileService fileService,
            IBrowseService browseService,
            ILogService log)
        {
            _controlsService = controlService;
            _fileService = fileService;
            _browseService = browseService;
            _log = log;
        }

        public void ExecuteStopButton()
        {
            _log.Called(); //log

            try
            {
                _browseService.CancelCollectData(); //cancel
            }
            catch (Exception e)
            {
                _log.Error(e.Message); //log
            }
            finally
            {
                _controlsService.GetStoppedConfiguration(false); //ui
            }
        }

        public void ExecutePauseButton(bool paused)
        {
            _log.Called(paused); //log

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
            }
            catch (Exception e)
            {
                _log.Error(e.Message); //log
            }
        }

        public async Task ExecuteStartButtonAsync(List<string> nameList, IWpfWebBrowser webBrowser, string searchPhrase, bool paused)
        {
            _log.Called(nameList.Count, string.Empty, searchPhrase, paused); //log

            try
            {
                _controlsService.GetStartedConfiguration(); //ui

                if (!paused)
                {
                    await _browseService.CollectDataAsync(nameList, webBrowser, searchPhrase); //browser service
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message); //log
            }
            finally
            {
                if (!paused)
                {
                    _controlsService.GetStoppedConfiguration(false); //ui
                }
            }
        }

        public async Task<List<string>> ExecuteUploadButtonAsync(string filePath, bool init)
        {
            _log.Called(filePath, init); //log
            _log.Info("Looking good!");

            List<string> nameList = new List<string>();

            try
            {

                if (!string.IsNullOrEmpty(filePath))
                {
                    _controlsService.GetWaitConfiguration(); //ui

                    nameList = await _fileService.GetNewListFromFileAsync(filePath); //file

                    await Task.Delay(1000); //display messge

                    _controlsService.GetStoppedConfiguration(init); //ui

                    if (nameList.Count > 0)
                    {
                        _browseService.CancelCollectData(); //cancel

                        _browseService.SaveFilePath(filePath); //browser service

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
                _log.Error(e.Message); //log

                return new List<string>();
            }
        }

        public void ExecuteBuildButton(string searchPhrase)
        {
            _log.Called(searchPhrase); //log

            try
            {
                if (!string.IsNullOrEmpty(searchPhrase))
                {
                    _browseService.UpdateSearchPhrase(searchPhrase); //browser service
                }
                else
                {
                    throw new Exception("Incorrect phrase.");
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message); //log
            }
        }

        public async Task ExecuteConnectionButtonAsync(IWpfWebBrowser webBrowser, bool paused)
        {
            _log.Called(string.Empty, paused); //log

            try
            {
                _controlsService.GetWaitConfiguration();

                _browseService.ChangeConnectionType(webBrowser); //browser service

                await Task.Delay(1000); //display messge

                if (paused)
                {
                    _controlsService.GetPausedConfiguration(); //ui
                }
                else
                {
                    _controlsService.GetStoppedConfiguration(false); //ui
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message); //log
            }
        }

        public void ExecuteClickerChangeButton()
        {
            _log.Called(); //log

            try
            {
                _browseService.ClickerChange(); //browser service
            }
            catch (Exception e)
            {
                _log.Error(e.Message); //log
            }
        }

        public void ExecuteChangeIpButtonAsync(IWpfWebBrowser webBrowser)
        {
            _log.Called(); //log

            try
            {
                _browseService.GetNewIp(webBrowser); //browser service
            }
            catch (Exception e)
            {
                _log.Error(e.Message); //log
            }
        }
    }
}
