using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Messages;
using G_Hoover.Services.Scrap;
using NLog;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace G_Hoover.Services.Browsing
{
    public class BrowsingService : IBrowsingService
    {
        private readonly Logger _logger;
        private readonly IMessageService _messageService;
        private readonly IControlsService _controlsService;
        private readonly IScrapService _scrapService;
        private readonly IEventAggregator _eventAggregator;
        EventHandler<LoadingStateChangedEventArgs> _pageLoadedEventHandler;
        EventHandler<LoadingStateChangedEventArgs> _resultLoadedEventHandler;

        public BrowsingService(IMessageService messageService,
            IControlsService controlsService,
            IEventAggregator eventAggregator,
            IScrapService scrapService)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _messageService = messageService;
            _controlsService = controlsService;
            _scrapService = scrapService;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<UpdateControlsEvent>().Subscribe(OnUpdateControls);

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
        public int PhraseNo { get; set; } //number of currently checked phrase
        public CancellationTokenSource TokenSource { get; set; } //for cancellation
        public CancellationToken CancellationToken { get; set; } //cancellation token
        public int AudioTryCounter { get; set; } //how many times was solved audio captcha for one phrase
        public bool Paused { get; set; } //is paused by the user?
        public bool Stopped { get; set; } //is stopped by the user?
        public List<string> NameList { get; set; } //list of phrases loaded from the file
        public string SearchPhrase { get; set; } //phrase built in dialog window
        public IWpfWebBrowser WebBrowser { get; set; }

        private bool _clickerInput;
        public bool ClickerInput //if click by input simulation
        {
            get => _clickerInput;
            set
            {
                _clickerInput = value;
                CheckConditions();
            }
        }

        public async Task CollectDataAsync(List<string> nameList, IWpfWebBrowser webBrowser, string searchPhrase)
        {
            WebBrowser = webBrowser;
            SearchPhrase = searchPhrase;
            NameList = nameList;
            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;
            string callerName = nameof(CollectDataAsync);

            _logger.Info(MessagesInfo[callerName] + nameList.Count + ";" + searchPhrase); //log

            _controlsService.GetStartedConfiguration(); //ui

            Task task = Task.Run(async () =>
            {
                CancellationToken.ThrowIfCancellationRequested();

                await GetRecordAsync();

                _logger.Info(MessagesResult[callerName] + TokenSource.IsCancellationRequested); //log

            }, TokenSource.Token);

            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
            finally
            {
                TokenSource.Dispose();

                PhraseNo = 0;
            }
        }

        public async Task GetRecordAsync()
        {
            string callerName = nameof(GetRecordAsync);

            _logger.Info(MessagesInfo[callerName]); //log

            CheckConditions();

            AudioTryCounter = 0;
            string nextResult = string.Empty;

            while (Paused)
                Thread.Sleep(50);

            if (CancellationToken.IsCancellationRequested)
            {
                CancellationToken.ThrowIfCancellationRequested();
            }

            if (NameList.Count > PhraseNo)
            {
                await Task.Delay(100);
                string phrase = SearchPhrase.Replace("<name>", NameList[PhraseNo]);

                nextResult = await ContinueCrawling(phrase);

                if (!string.IsNullOrEmpty(nextResult))
                {
                    _logger.Info(MessagesResult[callerName] + PhraseNo); //log

                    PhraseNo++;
                }
                else
                {
                    _logger.Error(MessagesError[callerName]); //log
                }

                await GetRecordAsync();
            }
            else
            {
                //no more phrases / finalization
            }
        }

        public async Task<string> ContinueCrawling(string phrase)
        {
            string callerName = nameof(ContinueCrawling);

            _logger.Info(MessagesInfo[callerName] + phrase); //log

            bool clickSearch = false;
            bool isCaptcha = false;
            string result = "";

            bool loadingPage = true;

            WebBrowser.Load("https://www.google.com/");

            _pageLoadedEventHandler = async (sender, args) =>
            {
                if (args.IsLoading == false)
                {
                    WebBrowser.LoadingStateChanged -= _pageLoadedEventHandler;

                    while (Paused)
                        Thread.Sleep(50);

                    bool phraseEntered = await _scrapService.EnterPhraseAsync(ClickerInput, WebBrowser, phrase);

                    while (Paused)
                        Thread.Sleep(50);

                    //if phrase entered correctly
                    if (phraseEntered)
                    {
                        clickSearch = await _scrapService.CliskSearchBtnAsync(ClickerInput, WebBrowser);
                    }
                    else
                    {
                        loadingPage = false; //return string.Empty;
                    }

                    while (Paused)
                        Thread.Sleep(50);

                    //if button clicked correctly
                    if (clickSearch)
                    {
                        isCaptcha = await CheckResultPageAsync();
                    }
                    else
                    {
                        loadingPage = false; //return string.Empty;
                    }

                    while (Paused)
                        Thread.Sleep(50);

                    //if found captcha
                    if (!isCaptcha)
                    {
                        //collect result => await _scrapService.

                        _logger.Info(MessagesResult[callerName]); //log
                    }
                    else
                    {
                        //resolve / ClickerInput / checkconditions => await _scrapService.
                        //after resolving?? return empty, update counter for captcha ++ and try again??
                    }

                    await Task.Delay(1000);

                    loadingPage = false; //finished crawling
                }
            };

            WebBrowser.LoadingStateChanged += _pageLoadedEventHandler;

            while (loadingPage)
                await Task.Delay(50); //if still crawling

            _logger.Error(MessagesError[callerName]); //log

            return string.Empty;
        }

        public async Task<bool> CheckResultPageAsync()
        {
            bool loadingPage = true;
            bool isCaptcha = false;
            string callerName = nameof(CheckResultPageAsync);

            _logger.Info(MessagesInfo[callerName]); //log

            _resultLoadedEventHandler = async (sender, args) =>
            {
                if (args.IsLoading == false)
                {
                    isCaptcha = await _scrapService.CheckForRecaptchaAsync(WebBrowser);

                    if (!isCaptcha)
                    {
                        _logger.Info(MessagesResult[callerName] + isCaptcha); //log
                    }
                    else
                    {
                        _logger.Info(MessagesError[callerName] + isCaptcha); //log
                    }

                    loadingPage = false;
                }
            };

            WebBrowser.LoadingStateChanged += _resultLoadedEventHandler;

            while (loadingPage)
                await Task.Delay(50); //if still crawling

            return isCaptcha;
        }

        public void CheckConditions()
        {
            string callerName = nameof(CheckConditions);

            _logger.Info(MessagesInfo[callerName]); //log

            if (!ClickerInput) //if input clicker
            {
                _logger.Info(MessagesResult[callerName] + ClickerInput + ";" + Stopped); //log

                _controlsService.ShowLessBrowser();
            }
            else //if JavaScript clicker
            {
                _logger.Info(MessagesResult[callerName] + ClickerInput + ";" + Stopped); //log

                _controlsService.ShowMoreBrowser();
            }

            if (Stopped)
            {
                PhraseNo = 0;
                Stopped = false;
            }
        }

        public void OnUpdateControls(UiPropertiesModel obj)
        {
            Paused = obj.Paused;
            Stopped = obj.Stopped;
        }

        public void CancelCollectData()
        {
            if (TokenSource != null)
            {
                TokenSource.Cancel(); //browser service
            }
        }

        public void UpdateSearchPhrase(string searchPhrase)
        {
            SearchPhrase = searchPhrase;
        }
    }
}
