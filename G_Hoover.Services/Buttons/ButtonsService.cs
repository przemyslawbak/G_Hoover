using CefSharp.Wpf;
using G_Hoover.Services.Browsing;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using Params_Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G_Hoover.Services.Buttons
{
    public class ButtonsService : IButtonsService
    {
        private readonly IControlsService _controlsService;
        private readonly IFilingService _fileService;
        private readonly IBrowseService _browseService;
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();

        public ButtonsService(IControlsService controlService,
            IFilingService fileService,
            IBrowseService browseService)
        {
            _controlsService = controlService;
            _fileService = fileService;
            _browseService = browseService;
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

            List<string> nameList = new List<string>();

            try
            {
                _controlsService.GetWaitConfiguration(); //ui

                if (!string.IsNullOrEmpty(filePath))
                {

                    nameList = await _fileService.GetNewListFromFileAsync(filePath); //file

                    await Task.Delay(1000); //display messge

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
            finally
            {
                _controlsService.GetStoppedConfiguration(init); //ui
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

        public async Task ExecuteChangeIpButtonAsync(IWpfWebBrowser webBrowser, bool paused)
        {
            _log.Called(); //log

            try
            {
                _controlsService.GetWaitConfiguration();

                _browseService.GetNewIp(webBrowser); //browser service

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
    }
}
