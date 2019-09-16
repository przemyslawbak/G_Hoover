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

        /// <summary>
        /// cancelling data collecting, in finally block calls GetStoppedConfiguration
        /// stops browsing
        /// </summary>
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

        /// <summary>
        /// if is paused, GetStartedConfiguration, if is not paused, GetPausedConfiguration
        /// pauses (or restarts) browsing
        /// </summary>
        /// <param name="paused">is browsing paused bool</param>
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

        /// <summary>
        /// calls GetStartedConfiguration, if browsing is not paused, calls CollectDataAsync, in finally block calls GetStoppedConfiguration
        /// starts (or restarts) browsing
        /// </summary>
        /// <param name="nameList">list of searched names</param>
        /// <param name="webBrowser">cefsharp browser interface</param>
        /// <param name="searchPhrase">build searched phrase</param>
        /// <param name="paused">is browsing paused bool</param>
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

        /// <summary>
        /// for processing time calls GetWaitConfiguration, if file path is not null/empty calls GetNewListFromFileAsync, if list not empty calls CancelCollectData and SaveFilePath
        /// uploads new phrases
        /// </summary>
        /// <param name="filePath">path to the file with names</param>
        /// <param name="init">is it application init bool</param>
        /// <returns></returns>
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

        /// <summary>
        /// updates built search phrase in browse service
        /// </summary>
        /// <param name="searchPhrase">build searched phrase</param>
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

        /// <summary>
        /// for processing time calls GetWaitConfiguration, calls ChangeConnectionType, after this calls GetPausedConfiguration or GetStoppedConfiguration depending on paused bool
        /// changes type of connection
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        /// <param name="paused">is browsing paused bool</param>
        /// <returns></returns>
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

        /// <summary>
        /// calls ClickerChange for change of clicker type
        /// </summary>
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

        /// <summary>
        /// for processing time calls GetWaitConfiguration, calls GetNewIp
        /// changes browing IP, or initializes Tor if direct connection, after this calls GetPausedConfiguration or GetStoppedConfiguration depending on paused bool
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        /// <param name="paused">is browsing paused bool</param>
        /// <returns></returns>
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
