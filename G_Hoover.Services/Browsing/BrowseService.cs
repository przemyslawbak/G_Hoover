﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Audio;
using G_Hoover.Services.Config;
using G_Hoover.Services.Connection;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using G_Hoover.Services.Scrap;
using Params_Logger;
using Prism.Events;

namespace G_Hoover.Services.Browsing
{
    /// <summary>
    /// a service class browsing the search website
    /// </summary>
    public class BrowseService : IBrowseService
    {
        private readonly IAppConfig _config;
        private readonly IControlsService _controlsService;
        private readonly IScrapService _scrapService;
        private readonly IFilingService _fileService;
        private readonly IConnectionService _connectionService;
        private readonly IAudioService _audioService;
        private readonly IEventAggregator _eventAggregator;
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();

        EventHandler<LoadingStateChangedEventArgs> _pageLoadedEventHandler;
        EventHandler<LoadingStateChangedEventArgs> _resultLoadedEventHandler;
        private readonly int _audioTrialsLimit;
        private readonly int _torSearchesCaptchaLimit;

        public BrowseService(IControlsService controlsService,
            IEventAggregator eventAggregator,
            IScrapService scrapService,
            IFilingService fileService,
            IConnectionService connectionService,
            IAudioService audioService,
            IAppConfig config)
        {
            _config = config;
            _controlsService = controlsService;
            _scrapService = scrapService;
            _fileService = fileService;
            _connectionService = connectionService;
            _audioService = audioService;
            _eventAggregator = eventAggregator;

            if (Debugger.IsAttached) //save only for DEBUG
            {
                _audioTrialsLimit = 10;
                _torSearchesCaptchaLimit = 10;
            }
            else
            {
                _audioTrialsLimit = 3;
                _torSearchesCaptchaLimit = 2;
            }

            _eventAggregator.GetEvent<UpdateControlsEvent>().Subscribe(OnUpdateControls);
        }

        /// <summary>
        /// creates new cancellation token, calls LoopCollectingAsync
        /// finally disposes cancellation token
        /// </summary>
        /// <param name="nameList">list of names/phrases</param>
        /// <param name="webBrowser">cefsharp browser interface</param>
        /// <param name="searchPhrase">built search phrase</param>
        public async Task CollectDataAsync(List<string> nameList, IWpfWebBrowser webBrowser, string searchPhrase)
        {
            _log.Called(nameList.Count, string.Empty, searchPhrase);

            if (StopTS == null || StopTS.IsCancellationRequested) //if cancellation token is null or requested, create new one
            {
                StopTS = new CancellationTokenSource();
                StopCT = StopTS.Token;

                WebBrowser = webBrowser;
                SearchPhrase = searchPhrase;
                NameList = nameList;
                PhraseNo = GetPhraseNo();
            }

            Task task = Task.Run(async () =>
            {
                await LoopCollectingAsync();

            }, StopCT);

            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                _log.Info("Cancellation token cancelled: " + e.Message);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
            finally
            {
                StopTS.Dispose();
            }

            StopTS = null;
        }

        /// <summary>
        /// loops for GetNewRecordAsync, resets InputCorrection
        /// </summary>
        public async Task LoopCollectingAsync()
        {
            try
            {
                InputCorrection = false;
                await GetNewRecordAsync();
                await LoopCollectingAsync();
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// gets new record to be saved
        /// </summary>
        public async Task GetNewRecordAsync()
        {
            _log.Called();

            string phrase = SearchPhrase.Replace("<name>", NameList[PhraseNo]);
            LoadingPage = true;
            HowManySearches++;

            WebBrowser.Load("https://www.google.com/");

            _pageLoadedEventHandler = async (sender, args) =>
            {
                if (args.IsLoading == false)
                {
                    WebBrowser.LoadingStateChanged -= _pageLoadedEventHandler;

                    try
                    {
                        if (StopCT.IsCancellationRequested) StopCT.ThrowIfCancellationRequested(); //cancellation

                        Pause(); //if paused

                        await _scrapService.EnterPhraseAsync(ClickerInput, WebBrowser, phrase);

                        Pause(); //if paused

                        await _scrapService.ClickSearchBtnAsync(ClickerInput, WebBrowser);

                        Pause(); //if paused

                        bool isCaptcha = await CheckForCaptchaAsync();

                        Pause(); //if paused

                        if (!isCaptcha)
                        {
                            await GetAndSaveResultAsync();
                        }
                        else
                        {
                            throw new Exception("Captcha detected");
                        }

                        Pause(); //if paused
                    }
                    catch (OperationCanceledException e)
                    {
                        _log.Info("Method STOP cancelled: " + e.Message);
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "Captcha detected" && !StopCT.IsCancellationRequested)
                        {
                            _log.Info(e.Message);

                            ClickerInput = true;

                            await ResolveCaptchaAsync();

                            ClickerInput = false;
                        }
                        else
                        {
                            _log.Error(e.Message);
                        }
                    }
                    finally
                    {
                        LoadingPage = false;
                    }
                }
            };

            WebBrowser.LoadingStateChanged += _pageLoadedEventHandler;

            while (LoadingPage)
                await Task.Delay(50); //if still crawling
        }

        /// <summary>
        /// resolves captcha if found
        /// </summary>
        public async Task ResolveCaptchaAsync()
        {
            AudioTrials = 0;

            _log.Called();

            try
            {
                if (StopCT.IsCancellationRequested) StopCT.ThrowIfCancellationRequested(); //cancellation

                VerifyClickerInput();

                await _scrapService.TurnOffAlertsAsync(WebBrowser);

                if (SearchViaTor && HowManySearches == _torSearchesCaptchaLimit)
                {
                    GetNewIp(WebBrowser);
                }
                else
                {
                    Pause(); //if paused

                    await _scrapService.ClickCheckboxIcon(ClickerInput);

                    Pause(); //if paused

                    await Task.Delay(10000); //wait for pics to load JS pics

                    if (StopCT.IsCancellationRequested) StopCT.ThrowIfCancellationRequested(); //cancellation

                    bool isCaptcha = await _scrapService.CheckForRecaptchaAsync(WebBrowser);

                    if (isCaptcha)
                    {
                        await _scrapService.ClickAudioChallangeIconAsync(ClickerInput);

                        await ResolveAudioChallengeAsync();
                    }
                    else
                    {
                        await GetAndSaveResultAsync();
                    }
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// sets new browsing IP
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        public void GetNewIp(IWpfWebBrowser webBrowser)
        {
            _log.Called(string.Empty);

            try
            {
                if (SearchViaTor)
                {
                    _connectionService.InitializeTor();
                }
                else
                {
                    ChangeConnectionType(webBrowser);
                }

                HowManySearches = 0;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// resolves audio captcha
        /// </summary>
        public async Task ResolveAudioChallengeAsync()
        {
            _log.Called();

            try
            {
                Pause(); //if paused

                await Task.Delay(5000); //wait to load JS window

                if (StopCT.IsCancellationRequested) StopCT.ThrowIfCancellationRequested(); //cancellation

                bool isCaptcha = await _scrapService.CheckForRecaptchaAsync(WebBrowser);

                if (isCaptcha)
                {
                    if (AudioTrials == _audioTrialsLimit)
                    {
                        if (SearchViaTor)
                        {
                            GetNewIp(WebBrowser);
                        }
                        else
                        {
                            ChangeConnectionType(WebBrowser);
                        }
                    }
                    else
                    {

                        await RecordAndProcessAudioAsync();

                        AudioTrials++;

                        _log.Info(AudioTrials.ToString());
                    }
                }
                else
                {
                    //log that no more captcha
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// recording and processing audio
        /// </summary>
        public async Task RecordAndProcessAudioAsync()
        {
            _log.Called();

            try
            {
                Pause(); //if paused

                if (StopCT.IsCancellationRequested) StopCT.ThrowIfCancellationRequested(); //cancellation

                await _scrapService.ClickPlayIconAsync(ClickerInput, InputCorrection);

                await _audioService.RecordAudioSampleAsync();

                string audioResult = await _audioService.ProcessAudioSampleAsync();

                if (string.IsNullOrEmpty(audioResult))
                {
                    //log

                    await MakeNewAudioChallengeAsync();
                }
                else
                {
                    //log

                    await SendAudioResultAsync(audioResult);
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// sending audio result
        /// </summary>
        /// <param name="audioResult">result of audio processing</param>
        public async Task SendAudioResultAsync(string audioResult)
        {
            _log.Called(audioResult);

            try
            {
                if (StopCT.IsCancellationRequested) StopCT.ThrowIfCancellationRequested(); //cancellation

                audioResult = _fileService.ProsessText(audioResult);

                Pause(); //if paused

                await _scrapService.ClickTextBoxAsync(ClickerInput);

                Pause(); //if paused

                await _scrapService.EnterResultAsync(ClickerInput, audioResult);

                await Task.Delay(6000); //wait for audio challenge result

                bool isCaptcha = await _scrapService.CheckForRecaptchaAsync(WebBrowser);

                if (isCaptcha)
                {
                    InputCorrection = true;

                    await ResolveAudioChallengeAsync();
                }
                else
                {
                    //log

                    await GetAndSaveResultAsync();
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// clicks to get new audio challenge
        /// </summary>
        public async Task MakeNewAudioChallengeAsync()
        {
            InputCorrection = false;

            Pause(); //if paused

            await _scrapService.ClickNewAudioChallengeAsync(ClickerInput, InputCorrection);

            await RecordAndProcessAudioAsync();
        }

        /// <summary>
        /// scraps result and saves in the result file
        /// </summary>
        public async Task GetAndSaveResultAsync()
        {
            ResultObjectModel result = new ResultObjectModel();

            result = await CollectResultsAsync();

            if (string.IsNullOrEmpty(result.Url) && string.IsNullOrEmpty(result.Header))
            {
                ResolveEmptyString();
            }
            else
            {
                await _fileService.SaveNewResultAsync(result, NameList[PhraseNo]);

                PhraseNo++; //move to the next phrase
            }
        }

        /// <summary>
        /// pause browsing
        /// </summary>
        public void Pause()
        {
            _log.Info("Paused");

            while (Paused || PleaseWaitVisible)
                Thread.Sleep(50);

            _log.Info("Unpaused");
        }

        /// <summary>
        /// changes input or connection if result is empty string
        /// </summary>
        public void ResolveEmptyString()
        {
            if (!ClickerInput)
            {
                ClickerInput = true;
            }
            else if (!SearchViaTor)
            {
                ChangeConnectionType(WebBrowser);
            }
            else
            {
                PhraseNo++; //move to next
                ChangeConnectionType(WebBrowser);
                ClickerInput = false;
            }
        }

        /// <summary>
        /// collects result form html document
        /// </summary>
        /// <returns>ResultObjectModel</returns>
        public async Task<ResultObjectModel> CollectResultsAsync()
        {
            ResultObjectModel result = new ResultObjectModel
            {
                Header = await _scrapService.GetHeaderAsync(WebBrowser),
                Url = await _scrapService.GetUrlAsync(WebBrowser)
            };

            return result;
        }

        /// <summary>
        /// checks for captcha in html document
        /// </summary>
        /// <returns>captcha found bool</returns>
        public async Task<bool> CheckForCaptchaAsync()
        {
            bool loadingPage = true;
            bool isCaptcha = false;

            _resultLoadedEventHandler = async (sender, args) =>
            {
                if (args.IsLoading == false)
                {
                    isCaptcha = await _scrapService.CheckForRecaptchaAsync(WebBrowser);

                    loadingPage = false;
                }
            };

            WebBrowser.LoadingStateChanged += _resultLoadedEventHandler;

            while (loadingPage)
                await Task.Delay(50); //if still crawling

            await Task.Delay(1000);

            return isCaptcha;
        }

        /// <summary>
        /// changes connection type to Tor or direct
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        public void ChangeConnectionType(IWpfWebBrowser webBrowser)
        {
            if (!SearchViaTor)
            {
                _connectionService.ConfigureBrowserTor(webBrowser);
                SearchViaTor = true;
            }
            else
            {
                _connectionService.ConfigureBrowserDirect(webBrowser);
                SearchViaTor = false;
            }
            HowManySearches = 0;
        }

        /// <summary>
        /// checks for current clicker type, depending on this setting up browser properties
        /// </summary>
        public void VerifyClickerInput()
        {
            if (!ClickerInput) //if input clicker
            {
                _controlsService.ShowLessBrowser();
            }
            else if (ClickerInput && !Paused && !Stopped) //if JavaScript clicker and not paused and stopped or working
            {
                _controlsService.ShowMoreBrowserRunning();
            }
            else if (ClickerInput && (Paused || Stopped || PleaseWaitVisible)) //if JavaScript clicker and paused or stopped
            {
                _controlsService.ShowMoreBrowserPaused();
            }
        }

        /// <summary>
        /// depending on current StatusPropertiesModel updates display properties
        /// </summary>
        public void UpdateStatusProperties()
        {
            StatusPropertiesModel status = new StatusPropertiesModel()
            {
                Clicker = ClickerInput ? "Input Simulator" : "JavaScript",
                Connection = SearchViaTor ? "Tor network" : "Direct connection",
                PhraseNo = PhraseNo
            };
            if (Paused)
                status.Status = "Paused";
            else if (Stopped)
                status.Status = "Stopped";
            else if (PleaseWaitVisible)
                status.Status = "Waiting";
            else
                status.Status = "Running";

            _eventAggregator.GetEvent<UpdateStatusEvent>().Publish(status);
        }

        /// <summary>
        /// updates controls by the event
        /// </summary>
        public void OnUpdateControls(UiPropertiesModel obj)
        {
            Paused = obj.Paused;
            Stopped = obj.Stopped;
            PleaseWaitVisible = obj.PleaseWaitVisible;

            if (Stopped && !obj.Init)
            {
                PhraseNo = 0;
            }

            UpdateStatusProperties();
            VerifyClickerInput();
        }

        /// <summary>
        /// cancellation of the token and stops loading website if currently loading any
        /// </summary>
        public void CancelCollectData()
        {
            if (StopTS != null && !StopTS.IsCancellationRequested)
            {
                if (WebBrowser.IsLoading)
                {
                    WebBrowser.Stop();
                }

                StopTS.Cancel(); //cancel browsing
            }
        }

        /// <summary>
        /// updating SearchPhrase prop
        /// </summary>
        /// <param name="searchPhrase">string built phrase</param>
        public void UpdateSearchPhrase(string searchPhrase)
        {
            SearchPhrase = searchPhrase;
            _config.SaveSearchPhrase(searchPhrase);
        }

        /// <summary>
        /// saves PhraseNo prop into config file
        /// </summary>
        public void SavePhraseNo()
        {
            _config.SavePhraseNo(PhraseNo);
        }

        /// <summary>
        /// gets PhraseNo from config file
        /// </summary>
        public int GetPhraseNo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            int toReturn = int.Parse(config.AppSettings.Settings["phraseNo"].Value);

            return toReturn;
        }

        /// <summary>
        /// save path to the file with list of names/phrases
        /// </summary>
        /// <param name="filePath">string path</param>
        public void SaveFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                _config.SaveFilePath(filePath);
            }
        }

        /// <summary>
        /// changes clicker type
        /// </summary>
        public void ClickerChange()
        {
            ClickerInput = ClickerInput ? false : true;
        }

        public int AudioTrials { get; set; } //number of currently audio challenge trials for this phrase
        public int HowManySearches { get; set; } //how many searches for this IP
        public CancellationTokenSource StopTS { get; set; } //for cancellation
        public CancellationToken StopCT { get; set; } //cancellation token
        public bool Paused { get; set; } //is paused by the user?
        public bool Stopped { get; set; } //is stopped by the user?
        public bool PleaseWaitVisible { get; set; } //is work in progress?
        public List<string> NameList { get; set; } //list of phrases loaded from the file
        public string SearchPhrase { get; set; } //phrase built in dialog window
        public IWpfWebBrowser WebBrowser { get; set; } //passed web browser instance from VM
        public bool LoadingPage { get; set; } //if page is now loading
        public bool InputCorrection { get; set; } //if need to add correction for input
        public string Status { get; set; } //current browsing status

        private int _phraseNo;
        public int PhraseNo //number of currently checked phrase
        {
            get => _phraseNo;
            set
            {
                _phraseNo = value;
                SavePhraseNo(); //save phraseno to cofig file
                UpdateStatusProperties(); //publishes event with status model
            }
        }

        private bool _searchViaTor;
        public bool SearchViaTor //if Tor network in use
        {
            get => _searchViaTor;
            set
            {
                _searchViaTor = value;
                UpdateStatusProperties(); //publishes event with status model
            }
        }

        private bool _clickerInput;
        public bool ClickerInput //if click by input simulation
        {
            get => _clickerInput;
            set
            {
                _clickerInput = value;
                VerifyClickerInput(); //check window setup on prop update
                UpdateStatusProperties(); //publishes event with status model
            }
        }
    }
}
