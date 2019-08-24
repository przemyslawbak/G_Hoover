﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Audio;
using G_Hoover.Services.Config;
using G_Hoover.Services.Connection;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using G_Hoover.Services.Scrap;
using NLog;
using Prism.Events;

namespace G_Hoover.Services.Browsing
{
    public class BrowseService : IBrowseService
    {
        private readonly AppConfig _config;
        private readonly Logger _logger;
        private readonly IMessageService _messageService;
        private readonly IControlsService _controlsService;
        private readonly IScrapService _scrapService;
        private readonly IFileService _fileService;
        private readonly IConnectionService _connectionService;
        private readonly IAudioService _audioService;
        private readonly IEventAggregator _eventAggregator;
        EventHandler<LoadingStateChangedEventArgs> _pageLoadedEventHandler;
        EventHandler<LoadingStateChangedEventArgs> _resultLoadedEventHandler;
        private readonly int _audioTrialsLimit = 30; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! for testing only
        private readonly int _torSearchesCaptchaLimit = 20; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! for testing only

        public BrowseService(IMessageService messageService,
            IControlsService controlsService,
            IEventAggregator eventAggregator,
            IScrapService scrapService,
            IFileService fileService,
            IConnectionService connectionService,
            IAudioService audioService)
        {
            _config = new AppConfig();
            _logger = LogManager.GetCurrentClassLogger();
            _messageService = messageService;
            _controlsService = controlsService;
            _scrapService = scrapService;
            _fileService = fileService;
            _connectionService = connectionService;
            _audioService = audioService;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<UpdateControlsEvent>().Subscribe(OnUpdateControls);
        }

        public Dictionary<string, string> MessagesInfo { get; set; }
        public Dictionary<string, string> MessagesError { get; set; }
        public Dictionary<string, string> MessagesResult { get; set; }
        public Dictionary<string, string> MessagesDisplay { get; set; }
        public int AudioTrials { get; set; } //number of currently audio challenge trials for this phrase
        public int HowManySearches { get; set; } //how many searches for this IP
        public CancellationTokenSource StopTokenSource { get; set; } //for cancellation
        public CancellationToken StopCancellationToken { get; set; } //cancellation token
        public bool Paused { get; set; } //is paused by the user?
        public bool Stopped { get; set; } //is stopped by the user?
        public bool PleaseWaitVisible { get; set; } //is work in progress?
        public List<string> NameList { get; set; } //list of phrases loaded from the file
        public string SearchPhrase { get; set; } //phrase built in dialog window
        public IWpfWebBrowser WebBrowser { get; set; } //passed web browser instance from VM
        public bool LoadingPage { get; set; } //if page is now loading
        public bool InputCorrection { get; set; } //if need to add correction for input
        public string Status { get; set; }

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

        public async Task CollectDataAsync(List<string> nameList, IWpfWebBrowser webBrowser, string searchPhrase)
        {
            if (StopTokenSource == null || StopTokenSource.IsCancellationRequested)
            {
                StopTokenSource = new CancellationTokenSource();
                StopCancellationToken = StopTokenSource.Token;

                WebBrowser = webBrowser;
                SearchPhrase = searchPhrase;
                NameList = nameList;
                PhraseNo = GetPhraseNo();
            }

            Task task = Task.Run(async () =>
            {
                await LoopCollectingAsync();

            }, StopTokenSource.Token);

            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                //log
            }
            catch (Exception e)
            {
                //log
            }
            finally
            {
                StopTokenSource.Dispose();
            }

            StopTokenSource = null;
        }

        public async Task LoopCollectingAsync()
        {
            if (StopCancellationToken.IsCancellationRequested)
            {
                StopCancellationToken.ThrowIfCancellationRequested();
            }

            if (NameList.Count > PhraseNo)
            {
                InputCorrection = false;

                await GetNewRecordAsync();

                await LoopCollectingAsync(); //loop
            }
            else
            {
                CancelCollectData();

                //finish; GetStoppedConfiguration is in ButtonsService already
            }
        }

        public async Task GetNewRecordAsync()
        {
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
                        Pause(); //if paused

                        await _scrapService.EnterPhraseAsync(ClickerInput, WebBrowser, phrase);

                        Pause(); //if paused

                        await _scrapService.CliskSearchBtnAsync(ClickerInput, WebBrowser);

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
                    catch (Exception e)
                    {
                        if (e.Message == "Captcha detected")
                        {
                            //log

                            ClickerInput = true;

                            await ResolveCaptchaAsync();

                            ClickerInput = false;
                        }
                        else
                        {
                            //log
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

        public async Task ResolveCaptchaAsync()
        {
           AudioTrials = 0;

            VerifyClickerInput();

            await _scrapService.TurnOffAlertsAsync(WebBrowser);

            if (SearchViaTor && HowManySearches == _torSearchesCaptchaLimit)
            {
                GetNewIp();
            }
            else
            {
                Pause(); //if paused

                await _scrapService.ClickCheckboxIcon(ClickerInput);

                Pause(); //if paused

                await Task.Delay(5000); //wait for pics to load JS pics

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

        public void GetNewIp()
        {
            if (SearchViaTor)
            {
                _connectionService.GetNewBrowsingIp(WebBrowser);
            }
            else
            {
                ChangeConnectionType(WebBrowser);
            }

            HowManySearches = 0;
        }

        public async Task ResolveAudioChallengeAsync()
        {
            Pause(); //if paused

            await Task.Delay(5000); //wait to load JS window

            bool isCaptcha = await _scrapService.CheckForRecaptchaAsync(WebBrowser);

            if (isCaptcha)
            {
                if (AudioTrials == _audioTrialsLimit)
                {
                    if (SearchViaTor)
                    {
                        GetNewIp();
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
                }
            }
            else
            {
                //log that no more captcha
            }
        }

        public async Task RecordAndProcessAudioAsync()
        {
            Pause(); //if paused

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

        public async Task SendAudioResultAsync(string audioResult)
        {
            audioResult = _fileService.ProsessText(audioResult);

            Pause(); //if paused

            await _scrapService.ClickTextBoxAsync(ClickerInput);

            Pause(); //if paused

            await _scrapService.EnterResultAsync(ClickerInput, audioResult);

            Pause(); //if paused

            await _scrapService.ClickSendResultAsync(ClickerInput, audioResult);

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

        public async Task MakeNewAudioChallengeAsync()
        {
            InputCorrection = false;

            Pause(); //if paused

            await _scrapService.ClickNewAudioChallengeAsync(ClickerInput, InputCorrection);

            await RecordAndProcessAudioAsync();
        }

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

        public void Pause()
        {
            while (Paused || PleaseWaitVisible)
                Thread.Sleep(50);
        }

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

        public async Task<ResultObjectModel> CollectResultsAsync()
        {
            ResultObjectModel result = new ResultObjectModel
            {
                Header = await _scrapService.GetHeaderAsync(WebBrowser),
                Url = await _scrapService.GetUrlAsync(WebBrowser)
            };

            return result;
        }

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

        public void CancelCollectData()
        {
            if (StopTokenSource != null && !StopTokenSource.IsCancellationRequested)
            {
                StopTokenSource.Cancel(); //cancel browsing
            }
        }

        public void UpdateSearchPhrase(string searchPhrase)
        {
            SearchPhrase = searchPhrase;
            _config.SaveSearchPhrase(searchPhrase);
        }

        public void SavePhraseNo()
        {
            _config.SavePhraseNo(PhraseNo);
        }

        public int GetPhraseNo()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            int toReturn = int.Parse(config.AppSettings.Settings["phraseNo"].Value);

            return toReturn;
        }

        public void SaveFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                _config.SaveFilePath(filePath);
            }
        }

        public void ClickerChange()
        {
            ClickerInput = ClickerInput ? false : true;
        }
    }
}
