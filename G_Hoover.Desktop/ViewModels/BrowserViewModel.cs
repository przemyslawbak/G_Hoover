using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Desktop.Commands;
using G_Hoover.Desktop.Views;
using G_Hoover.Services.Browser;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using NLog;
using System;
using System.Collections.Generic;
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

        public BrowserViewModel(IFileService fileService, IDialogService dialogService, IMessageService messageService, IBrowserService browserService)
        {
            _dialogService = dialogService;
            _fileService = fileService;
            _browserService = browserService;
            _messageService = messageService;
            _logger = LogManager.GetCurrentClassLogger();

            StartCommand = new AsyncCommand(async () => await OnStartCommandAsync());
            StopCommand = new DelegateCommand(OnStopCommand);
            PauseCommand = new DelegateCommand(OnPauseCommand);
            ConnectionChangeCommand = new DelegateCommand(OnConnectionChangeCommand);
            ClickerChangeCommand = new DelegateCommand(OnClickerChangeCommand);
            UploadCommand = new AsyncCommand(async () => await OnUploadCommandAsync());
            BuildCommand = new AsyncCommand(async () => await OnBuildCommandAsync());

            NameList = new List<string>();

            InitializeBrowser();
        }

        /// <summary>
        /// loads message dictionaries from MessageService
        /// </summary>
        public void LoadDictionaries()
        {
            MessagesInfo = new Dictionary<string, string>();
            MessagesError = new Dictionary<string, string>();
            MessagesResult = new Dictionary<string, string>();

            MessagesInfo = _messageService.GetMessagesInfo();
            MessagesError = _messageService.GetMessagesError();
            MessagesResult = _messageService.GetMessagesResult();
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
            PleaseWaitVisible = false;
            UiButtonsEnabled = false;
            StopBtnEnabled = true;
            PauseBtnEnabled = true;
        }

        public void StoppedConfiguration()
        {
            WorkStatus = WorkStatus.Stop;
            PleaseWaitVisible = false;
            UiButtonsEnabled = true;
            StopBtnEnabled = false;
            PauseBtnEnabled = false;
        }

        public void StartedConfiguration()
        {
            WorkStatus = WorkStatus.Start;
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
            if (NameList.Count > 0)
            {
                StartedConfiguration();

                await GetRecordAsync();
            }
            else
            {
                //no more phrases / finalization
            }
        }

        public async Task GetRecordAsync()
        {
            AudioTryCounter = 0; //reset audio counter
            string phrase = SearchPhrase.Replace("<name>", NameList[PhraseNo]); //search phrase

            string nextResult = await _browserService.ContinueCrawling(WebBrowser, phrase);

            //PhraseNo++



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

        public async Task OnStartCommandAsync()
        {
            LoadDictionaries();
            CallerName = _messageService.GetCallerName();

            _logger.Info(MessagesInfo[CallerName]); //log

            try
            {
                await CollectDataAsync();

                LogAndMessage(MessagesResult[CallerName] + SearchPhrase); //log
            }
            catch (Exception e)
            {
                LogAndMessage(MessagesError[CallerName] + e.Message); //log
            }
        }

        public async Task OnBuildCommandAsync()
        {
            LoadDictionaries();
            CallerName = _messageService.GetCallerName();

            _logger.Info(MessagesInfo[CallerName]); //log

            try
            {
                SearchPhrase = ShowDialog(viewModel => _dialogService.ShowDialog<PhraseView>(this, viewModel));

                if (!string.IsNullOrEmpty(SearchPhrase))
                {
                    await _fileService.SavePhraseAsync(SearchPhrase);

                    LogAndMessage(MessagesResult[CallerName] + SearchPhrase); //log
                }
                else
                {
                    throw new Exception("Incorrect phrase.");
                }
            }
            catch (Exception e)
            {
                _logger.Error(MessagesError[CallerName] + e.Message); //log
            }

        }

        public async Task OnUploadCommandAsync()
        {
            LoadDictionaries();
            CallerName = _messageService.GetCallerName();
            NameList.Clear();

            _logger.Info(MessagesInfo[CallerName]); //log

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
                        LogAndMessage(MessagesResult[CallerName]); //log
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
                _logger.Error(MessagesError[CallerName] + e.Message); //log
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
            throw new NotImplementedException();
        }

        public void OnStopCommand(object obj)
        {
            throw new NotImplementedException();
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
        public string CallerName { get; set; } //caller method
        public string SearchPhrase { get; set; } //phrase built in dialog window
        public int AudioTryCounter { get; set; } //how many times was solved audio captcha for one phrase
        public WorkStatus WorkStatus { get; set; }

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
