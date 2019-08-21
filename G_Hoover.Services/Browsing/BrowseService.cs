using System;
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
        public bool SearchViaTor { get; set; } //if Tor network in use
        public bool LoadingPage { get; set; } //if page is now loading
        public bool IsCaptcha { get; set; }

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
            if (StopCancellationToken.IsCancellationRequested)
            {
                StopCancellationToken.ThrowIfCancellationRequested();
            }

            if (NameList.Count > PhraseNo)
            {
                AudioTrials = 0;

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
            IsCaptcha = false;
            bool isResult = false;
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

                        IsCaptcha = await CheckForCaptcha();

                        Pause(); //if paused

                        if (!IsCaptcha)
                        {
                            isResult = await GetAndSaveResultAsync();
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

                            await ResolveCaptcha();

                            if (!IsCaptcha) //if no need to solve captcha anymore
                            {
                                isResult = await GetAndSaveResultAsync();
                            }

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

                        if (isResult)
                        {
                            PhraseNo++; //move to the next phrase
                        }
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

            VerifyClickerInput();

            await _scrapService.TurnOffAlerts(WebBrowser);

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

                await _scrapService.ClickAudioChallangeIcon(ClickerInput);

                await ResolveAudioChallengeAsync();
            }
        }

        public void GetNewIp()
        {
            HowManySearches = 0;

            _connectionService.GetNewBrowsingIp(WebBrowser);
        }

        public async Task ResolveAudioChallengeAsync()
        {
            Pause(); //if paused

            await Task.Delay(5000); //wait to load JS window

            IsCaptcha = await _scrapService.CheckForRecaptchaAsync(WebBrowser);

            if (IsCaptcha)
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
            await _scrapService.ClickPlayIcon(ClickerInput);

            await _audioService.RecordAudioSampleAsync();

            string audioResult = await _audioService.ProcessAudioSampleAsync();

            //ifs for audioResult:
            //- get new audio challenge (input klicks)
            //- send result (input klicks)
        }

        public async Task<bool> GetAndSaveResultAsync()
        {
            ResultObjectModel result = new ResultObjectModel();

            result = await CollectResultsAsync();

            if (string.IsNullOrEmpty(result.Url) && string.IsNullOrEmpty(result.Header))
            {
                ResolveEmptyString();

                return false;
            }
            else
            {
                await _fileService.SaveNewResult(result, NameList[PhraseNo]);

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
                HowManySearches = 0;
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
    }
}
