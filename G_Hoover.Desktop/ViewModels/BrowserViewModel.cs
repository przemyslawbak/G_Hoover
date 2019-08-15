using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Desktop.Commands;
using G_Hoover.Desktop.Startup;
using G_Hoover.Desktop.Views;
using G_Hoover.Models;
using G_Hoover.Services.Browser;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public enum WorkStatus : byte { Start, Stop, Pause }
    public class BrowserViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly IBrowserService _browserService;
        private readonly IMessageService _messageService;
        private Logger _logger;
        private EventHandler<LoadingStateChangedEventArgs> _pageLoadedEventHandler;

        public BrowserViewModel(IFileService fileService, IDialogService dialogService, IBrowserService browserService, IMessageService messageService)
        {
            _dialogService = dialogService;
            _fileService = fileService;
            _browserService = browserService;
            _logger = LogManager.GetCurrentClassLogger();
            _messageService = messageService;

            StartCommand = new AsyncCommand(async () => await OnStartCommandAsync());
            StopCommand = new DelegateCommand(OnStopCommand);
            PauseCommand = new DelegateCommand(OnPauseCommand);
            ConnectionChangeCommand = new DelegateCommand(OnConnectionChangeCommand);
            ClickerChangeCommand = new DelegateCommand(OnClickerChangeCommand);
            UploadCommand = new AsyncCommand(async () => await OnUploadCommandAsync());
            BuildCommand = new AsyncCommand(async () => await OnBuildCommandAsync());

            NameList = new List<string>();

            LoadDictionaries();
            InitializeBrowser();
        }

        private void LoadDictionaries()
        {
            MessageDictionaries messages = _messageService.LoadDictionaries();

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
            WorkStatus = WorkStatus.Pause;
            Paused = true;
            PleaseWaitVisible = false;
            UiButtonsEnabled = true;
            StopBtnEnabled = true;
            PauseBtnEnabled = true;
        }

        public void StoppedConfiguration()
        {
            WorkStatus = WorkStatus.Stop;
            Paused = false;
            PleaseWaitVisible = false;
            UiButtonsEnabled = true;
            StopBtnEnabled = false;
            PauseBtnEnabled = false;
        }

        public void StartedConfiguration()
        {
            WorkStatus = WorkStatus.Start;
            Paused = false;
            PleaseWaitVisible = false;
            UiButtonsEnabled = false;
            StopBtnEnabled = true;
            PauseBtnEnabled = true;
            if (ClickerInput) //if input clicker
            {
                ShowAll();
            }
            else //if JavaScript clicker
            {
                ShowLess();
            }
        }

        public void PleaseWaitConfiguration()
        {
            PleaseWaitVisible = true;
            UiButtonsEnabled = false;
            StopBtnEnabled = false;
            PauseBtnEnabled = false;
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
            AudioTryCounter = 0;
            string nextResult = string.Empty;

            while (Paused)
                Thread.Sleep(100);

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

        public void LogAndMessage(string message)
        {
            _logger.Info(message);
            Message = _dialogService.ShowMessageBox(this, message);
        }

        public async Task OnStartCommandAsync()
        {
            string callerName = nameof(OnStartCommandAsync);
            _logger.Info(MessagesInfo[callerName]); //log

            try
            {
                await CollectDataAsync();

                _logger.Info(MessagesResult[callerName]); //log


            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
            finally
            {
                StoppedConfiguration();
            }
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
            string callerName = nameof(OnUploadCommandAsync);
            NameList.Clear();

            _logger.Info(MessagesInfo[callerName]); //log

            try
            {
                FilePath = GetFilePath();

                if (!string.IsNullOrEmpty(FilePath))
                {
                    PleaseWaitConfiguration(); //ui
                    NameList = await _fileService.GetNewListFromFileAsync(FilePath); //load collection
                    StoppedConfiguration(); //ui

                    if (NameList.Count > 0)
                    {
                        PhraseNo = 0;
                        _logger.Info(MessagesResult[callerName]); //log
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
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
        }

        public void OnClickerChangeCommand(object obj)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionChangeCommand(object obj)
        {
            throw new NotImplementedException();
        }

        public void OnPauseCommand(object obj)
        {
            if (Paused == true)
            {
                StartedConfiguration();
                Paused = false;
                //and browser service here;
            }
            else
            {
                PausedConfiguration();
                Paused = true;
                //and browser service here;
            }
        }

        public void OnStopCommand(object obj)
        {
            string callerName = nameof(OnStopCommand);

            _logger.Info(MessagesInfo[callerName]); //log

            try
            {
                TokenSource.Cancel();

                _logger.Info(MessagesResult[callerName]); //log
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[callerName] + e.Message); //log
            }
            finally
            {
                StoppedConfiguration();
            }
        }

        public IAsyncCommand StartCommand { get; private set; }
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
        public WorkStatus WorkStatus { get; set; } //work status

        private bool _paused;
        public bool Paused
        {
            get => _paused;
            set
            {
                _paused = value;
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
            get => _pauseBtnEnabled;
            set
            {
                _pauseBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _stopBtnEnabled;
        public bool StopBtnEnabled
        {
            get => _stopBtnEnabled;
            set
            {
                _stopBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _uiButtonsEnabled;
        public bool UiButtonsEnabled
        {
            get => _uiButtonsEnabled;
            set
            {
                _uiButtonsEnabled = value;
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
