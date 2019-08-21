﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Audio;
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
        private readonly int _audioTrialsLimit = 3;

        public BrowseService(IMessageService messageService,
            IControlsService controlsService,
            IEventAggregator eventAggregator,
            IScrapService scrapService,
            IFileService fileService,
            IConnectionService connectionService,
            IAudioService audioService)
        {
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
        public int PhraseNo { get; set; } //number of currently checked phrase
        public int AudioTrials { get; set; } //number of currently audio challenge trials
        public CancellationTokenSource StopTokenSource { get; set; } //for cancellation
        public CancellationToken StopCancellationToken { get; set; } //cancellation token
        public int AudioTryCounter { get; set; } //how many times was solved audio captcha for one phrase
        public bool Paused { get; set; } //is paused by the user?
        public bool Stopped { get; set; } //is stopped by the user?
        public bool PleaseWaitVisible { get; set; } //is work in progress?
        public List<string> NameList { get; set; } //list of phrases loaded from the file
        public string SearchPhrase { get; set; } //phrase built in dialog window
        public IWpfWebBrowser WebBrowser { get; set; } //passed web browser instance from VM
        public bool SearchViaTor { get; set; } //if Tor network in use
        public bool LoadingPage { get; set; } //if page is now loading

        private bool _clickerInput;
        public bool ClickerInput //if click by input simulation
        {
            get => _clickerInput;
            set
            {
                _clickerInput = value;
                VerifyClickerInput();
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

                PhraseNo = 0;
            }

            StopTokenSource = null;
        }

        public async Task LoopCollectingAsync()
        {

            CheckClickerConditions();

            if (StopCancellationToken.IsCancellationRequested)
            {
                StopCancellationToken.ThrowIfCancellationRequested();
            }

            if (NameList.Count > PhraseNo)
            {
                await GetNewRecordAsync();

                await LoopCollectingAsync(); //loop
            }
            else
            {
                CancelCollectData();
                //finish
            }
        }

        public async Task GetNewRecordAsync()
        {
            string phrase = SearchPhrase.Replace("<name>", NameList[PhraseNo]);
            LoadingPage = true;
            bool isCaptchaDetected = false;
            bool result = false;

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

                        isCaptchaDetected = await CheckForCaptcha();

                        Pause(); //if paused

                        if (!isCaptchaDetected)
                        {
                            result = await GetAndSaveResultAsync();
                        }
                        else
                        {
                            throw new Exception("Captcha detected");
                        }

                        Pause(); //if paused

                        if (result)
                        {
                            //log ok
                        }
                        else
                        {
                            throw new Exception("Result was empty");
                        }
                    }
                    catch (Exception e)
                    {
                        if (e.Message == "Result was empty")
                        {
                            //log

                            ResolveEmptyString();
                        }
                        else if (e.Message == "Captcha detected")
                        {
                            //log

                            ClickerInput = true;

                            await ResolveCaptcha();

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

        public async Task ResolveCaptcha()
        {
            AudioTrials = 0;

            CheckClickerConditions();

            await _scrapService.TurnOffAlerts(WebBrowser);

            if (SearchViaTor)
            {
                _connectionService.GetNewBrowsingIp(WebBrowser);
            }
            else
            {
                Pause(); //if paused

                await _scrapService.ClickCheckboxIcon(ClickerInput);

                Pause(); //if paused

                await Task.Delay(5000); //wait for pics to load JS pics

                await _scrapService.ClickAudioChallangeIcon(ClickerInput);

                await ResolveAudioChallengeAsync();
            }
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
                        _connectionService.GetNewBrowsingIp(WebBrowser);
                    }
                    else
                    {
                        ChangeConnectionType(WebBrowser);
                    }
                }
                else
                {
                    await RecordAudioSampleAsync();

                    AudioTrials++;
                }
            }
            else
            {
                //log that no more captcha
            }
        }

        public async Task RecordAudioSampleAsync()
        {
            await _scrapService.ClickPlayIcon(ClickerInput);

            await _audioService.RecordAudioSample();
        }

        public async Task<bool> GetAndSaveResultAsync()
        {
            ResultObjectModel result = new ResultObjectModel();

            result = await CollectResultsAsync();

            if (string.IsNullOrEmpty(result.Url) && string.IsNullOrEmpty(result.Header))
            {
                return false;
            }
            else
            {
                await _fileService.SaveNewResult(result, NameList[PhraseNo]);

                PhraseNo++; //move to next

                return true;
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
                Url = await _scrapService.GetUrl(WebBrowser)
            };

            return result;
        }

        public async Task<bool> CheckForCaptcha()
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
        }

        public void VerifyClickerInput()
        {
            if (!ClickerInput) //if input clicker
            {
                _controlsService.ShowLessBrowser();
            }
            else //if JavaScript clicker
            {
                _controlsService.ShowMoreBrowser();
            }
        }

        public void OnUpdateControls(UiPropertiesModel obj)
        {
            Paused = obj.Paused;
            Stopped = obj.Stopped;
            PleaseWaitVisible = obj.PleaseWaitVisible;
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
        }

        public void CheckClickerConditions()
        {
            VerifyClickerInput();
        }
    }
}
