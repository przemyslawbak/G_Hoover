using CefSharp.Wpf;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Messages;
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
        private readonly IEventAggregator _eventAggregator;

        public BrowsingService(IMessageService messageService, IControlsService controlsService, IEventAggregator eventAggregator)
        {
            _logger = LogManager.GetCurrentClassLogger();
            _messageService = messageService;
            _controlsService = controlsService;
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
        public bool ClickerInput { get; set; } //if click by input simulation
        public CancellationTokenSource TokenSource { get; set; } //for cancellation
        public CancellationToken CancellationToken { get; set; } //cancellation token
        public int AudioTryCounter { get; set; } //how many times was solved audio captcha for one phrase
        public bool Paused { get; set; } //is paused by the user?
        public bool Stopped { get; set; } //is stopped by the user?
        public List<string> NameList { get; set; } //list of phrases loaded from the file

        public async Task CollectDataAsync(List<string> nameList, IWpfWebBrowser webBrowser)
        {
            string callerName = nameof(CollectDataAsync);
            _logger.Info(MessagesInfo[callerName]); //log

            NameList = nameList;
            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;

            _controlsService.GetStartedConfiguration(); //ui

            Task task = Task.Run(async () =>
            {
                CancellationToken.ThrowIfCancellationRequested();

                await GetRecordAsync();

                _logger.Info(MessagesResult[callerName]); //log

            }, TokenSource.Token);

            try
            {
                await task;
            }
            catch (OperationCanceledException e)
            {
                _logger.Info(MessagesError[callerName] + e.Message); //log
            }
            finally
            {
                TokenSource.Dispose();

                PhraseNo = 0;
            }
        }

        public async Task GetRecordAsync()
        {
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
                await Task.Delay(1000);
                //string phrase = SearchPhrase.Replace("<name>", NameList[PhraseNo]);

                //nextResult = await _browserService.ContinueCrawling(WebBrowser, phrase);

                PhraseNo++;

                await GetRecordAsync();
            }
            else
            {
                //no more phrases / finalization
            }
        }

        private void CheckConditions()
        {
            if (ClickerInput) //if input clicker
            {
                ShowAll();
            }
            else //if JavaScript clicker
            {
                ShowLess();
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

        public void ShowLess()
        {
            _controlsService.ShowLessBrowser();
        }

        public void ShowAll()
        {
            _controlsService.ShowMoreBrowser();
        }

        public void CancelCollectData()
        {
            if (TokenSource != null)
            {
                TokenSource.Cancel(); //browser service
            }
        }
    }
}
