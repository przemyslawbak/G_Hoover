using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Desktop.Commands;
using G_Hoover.Desktop.Startup;
using G_Hoover.Desktop.Views;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Browser;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using NLog;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public class BrowserViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly IBrowserService _browserService;
        private readonly IMessageService _messageService;
        private readonly IControlsService _controlsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public BrowserViewModel(IFileService fileService,
            IDialogService dialogService,
            IBrowserService browserService,
            IMessageService messageService,
            IControlsService controlsService,
            IEventAggregator eventAggregator)
        {
            _dialogService = dialogService;
            _fileService = fileService;
            _browserService = browserService;
            _controlsService = controlsService;
            _eventAggregator = eventAggregator;
            _messageService = messageService;
            _logger = LogManager.GetCurrentClassLogger();

            StartCommand = new DelegateCommand(OnStartCommandAsync);
            StopCommand = new DelegateCommand(OnStopCommand);
            PauseCommand = new DelegateCommand(OnPauseCommand);
            ConnectionChangeCommand = new DelegateCommand(OnConnectionChangeCommand);
            ClickerChangeCommand = new DelegateCommand(OnClickerChangeCommand);
            UploadCommand = new AsyncCommand(async () => await OnUploadCommandAsync());
            BuildCommand = new AsyncCommand(async () => await OnBuildCommandAsync());

            NameList = new List<string>();
            UiControls = new UiPropertiesModel();

            _eventAggregator.GetEvent<UpdateControlsEvent>().Subscribe(OnUpdateControls);

            LoadDictionaries();
            InitializeBrowser();
        }

        private void LoadDictionaries()
        {
            MessageDictionariesModel messages = _messageService.LoadDictionaries();

            MessagesInfo = messages.MessagesInfo;
            MessagesError = messages.MessagesError;
            MessagesResult = messages.MessagesResult;
            MessagesDisplay = messages.MessagesDisplay;
        }

        public async void InitializeBrowser()
        {
            AudioTryCounter = 0;
            PhraseNo = 0;

            StoppedConfiguration();
            await LoadPhraseAsync();
            _fileService.RemoveOldLogs();
        }

        public async Task LoadPhraseAsync()
        {
            SearchPhrase = await _fileService.LoadPhraseAsync();
        }

        public void PausedConfiguration()
        {
            _controlsService.GetPausedConfiguration();
        }

        public void StoppedConfiguration()
        {
            _controlsService.GetStoppedConfiguration();
        }

        public void StartedConfiguration()
        {
            _controlsService.GetStartedConfiguration();
        }

        public void PleaseWaitConfiguration()
        {
            _controlsService.GetWaitConfiguration();
        }

        public async Task CollectDataAsync()
        {
            string callerName = nameof(CollectDataAsync);
            _logger.Info(MessagesInfo[callerName]); //log

            TokenSource = new CancellationTokenSource();
            CancellationToken = TokenSource.Token;

            StartedConfiguration();

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
                PhraseNo++;

                TokenSource.Dispose();
            }
        }

        public async Task GetRecordAsync()
        {
            if (ClickerInput) //if input clicker
            {
                ShowAll();
            }
            else //if JavaScript clicker
            {
                ShowLess();
            }
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

                /*
            WebBrowser.Load("https://www.google.com/"); //load website


            _pageLoadedEventHandler = async (sender, args) =>
            {
                if (args.IsLoading == false)
                {
                    WebBrowser.LoadingStateChanged -= _pageLoadedEventHandler;
                            await WebBrowser.EvaluateScriptAsync(@"
                            var arr = document.getElementsByTagName('input')[2].value = '" + phrase + @"';
                            "); await Task.Delay(1000);
                            await WebBrowser.EvaluateScriptAsync(@"
                            var arr = document.getElementsByTagName('input')[3].click();
                            ");
                }
            };

                WebBrowser.LoadingStateChanged += _pageLoadedEventHandler;
            */

                PhraseNo++;

                await GetRecordAsync();
            }
            else
            {
                //no more phrases / finalization
            }
        }

        private void ShowLess()
        {
            CurWindowState = WindowState.Normal;

            IsBrowserFocused = false;
        }

        private void ShowAll()
        {
            CurWindowState = WindowState.Maximized;

            IsBrowserFocused = true;

            WebBrowser.Focus();
        }

        public string GetFilePath()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = "This Is The Title",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            bool? success = _dialogService.ShowOpenFileDialog(this, settings);
            if (success == true)
            {
                return settings.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        public string ShowDialog(Func<PhraseViewModel, bool?> showDialog)
        {
            var dialogViewModel = new PhraseViewModel(SearchPhrase);

            bool? success = showDialog(dialogViewModel);
            if (success == true)
            {
                return dialogViewModel.Text;
            }
            else
            {
                return string.Empty;
            }
        }

        public void DisplayMessage(string message)
        {
            Message = _dialogService.ShowMessageBox(this, message);
        }

        private void OnUpdateControls(UiPropertiesModel obj)
        {
            UiControls = obj;
        }

        public void OnStartCommandAsync(object obj)
        {
            _controlsService.ExecuteStartButton();
        }

        public async Task OnBuildCommandAsync()
        {
            string callerName = nameof(OnBuildCommandAsync);
            _logger.Info(MessagesInfo[callerName]); //log

            try
            {
                SearchPhrase = ShowDialog(viewModel => _dialogService.ShowDialog<PhraseView>(this, viewModel));

                if (!string.IsNullOrEmpty(SearchPhrase))
                {
                    await _fileService.SavePhraseAsync(SearchPhrase);

                    _logger.Info(MessagesResult[callerName] + SearchPhrase); //log
                }
                else
                {
                    throw new Exception("Incorrect phrase.");
                }
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }

        }

        public async Task OnUploadCommandAsync()
        {
            FilePath = GetFilePath();

            await _controlsService.ExecuteUploadButtonAsync(FilePath);
        }

        public void OnPauseCommand(object obj)
        {
            _controlsService.ExecutePauseButton(Paused);
        }

        public void OnStopCommand(object obj)
        {
            _controlsService.ExecuteStopButton();
        }

        public void OnClickerChangeCommand(object obj)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionChangeCommand(object obj)
        {
            throw new NotImplementedException();
        }

        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand ConnectionChangeCommand { get; private set; }
        public ICommand ClickerChangeCommand { get; private set; }
        public IAsyncCommand UploadCommand { get; private set; }
        public IAsyncCommand BuildCommand { get; private set; }

        public bool ClickerInput { get; set; } //if click by input simulation
        public string CompletePhrase { get; set; } //search phrase builded
        public bool SearchViaTor { get; set; } //if searching when using Tor network
        public int PhraseNo { get; set; } //number of currently checked phrase
        public List<string> NameList { get; set; } //list of phrases loaded from the file
        public string FilePath { get; set; } //path of uploaded file
        public MessageBoxResult Message { get; set; } //error messages
        public Dictionary<string, string> MessagesInfo { get; set; } //message dictionary
        public Dictionary<string, string> MessagesError { get; set; } //message dictionary
        public Dictionary<string, string> MessagesResult { get; set; } //message dictionary
        public Dictionary<string, string> MessagesDisplay { get; set; } //message dictionary
        public string SearchPhrase { get; set; } //phrase built in dialog window
        public int AudioTryCounter { get; set; } //how many times was solved audio captcha for one phrase
        public CancellationTokenSource TokenSource { get; set; } //for cancellation
        public CancellationToken CancellationToken { get; set; } //cancellation token

        private UiPropertiesModel _uiControls;
        public UiPropertiesModel UiControls
        {
            get => _uiControls;
            set
            {
                _uiControls = value;
                Paused = _uiControls.Paused;
                PleaseWaitVisible = _uiControls.PleaseWaitVisible;
                UiButtonsEnabled = _uiControls.UiButtonsEnabled;
                StopBtnEnabled = _uiControls.StopBtnEnabled;
                PauseBtnEnabled = _uiControls.PauseBtnEnabled;
                OnPropertyChanged();
            }
        }

        private bool _paused;
        public bool Paused
        {
            get => UiControls.Paused;
            set
            {
                UiControls.Paused = value;
                OnPropertyChanged();
            }
        }

        private WindowState _curWindowState;
        public WindowState CurWindowState
        {
            get => _curWindowState;
            set
            {
                _curWindowState = value;
                OnPropertyChanged();
            }
        }

        private bool _isBrowserFocused;
        public bool IsBrowserFocused
        {
            get => _isBrowserFocused;
            set
            {
                _isBrowserFocused = value;
                OnPropertyChanged();
            }
        }

        private bool _pleaseWaitVisible;
        public bool PleaseWaitVisible
        {
            get => _pleaseWaitVisible;
            set
            {
                _pleaseWaitVisible = value;
                OnPropertyChanged();
            }
        }
        private bool _pauseBtnEnabled;
        public bool PauseBtnEnabled
        {
            get => UiControls.PauseBtnEnabled;
            set
            {
                UiControls.PauseBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _stopBtnEnabled;
        public bool StopBtnEnabled
        {
            get => UiControls.StopBtnEnabled;
            set
            {
                UiControls.StopBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _uiButtonsEnabled;
        public bool UiButtonsEnabled
        {
            get => UiControls.UiButtonsEnabled;
            set
            {
                UiControls.UiButtonsEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged();
            }
        }

        private IWpfWebBrowser _webBrowser;
        public IWpfWebBrowser WebBrowser
        {
            get => _webBrowser;
            set
            {
                _webBrowser = value;
                OnPropertyChanged();
            }
        }

        private string _network;
        public string Network
        {
            get => _network;
            set
            {
                _network = value;
                OnPropertyChanged();
            }
        }

        private string _clicker;
        public string Clicker
        {
            get => _clicker;
            set
            {
                _clicker = value;
                OnPropertyChanged();
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }
    }
}
