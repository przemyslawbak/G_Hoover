using CefSharp.Wpf;
using G_Hoover.Desktop.Commands;
using G_Hoover.Desktop.Views;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Buttons;
using G_Hoover.Services.Config;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public class BrowserViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IButtonsService _buttonService;
        private readonly IAppConfig _config;

        public BrowserViewModel(IFileService fileService,
            IDialogService dialogService,
            IMessageService messageService,
            IEventAggregator eventAggregator,
            IButtonsService buttonService,
            IAppConfig config)
        {
            _dialogService = dialogService;
            _fileService = fileService;
            _buttonService = buttonService;
            _eventAggregator = eventAggregator;
            _config = config;

            StartCommand = new AsyncCommand(async () => await OnStartCommandAsync());
            StopCommand = new DelegateCommand(OnStopCommand);
            PauseCommand = new DelegateCommand(OnPauseCommand);
            ConnectionChangeCommand = new AsyncCommand(async () => await OnConnectionChangeCommandAsync());
            ClickerChangeCommand = new DelegateCommand(OnClickerChangeCommand);
            UploadCommand = new AsyncCommand(async () => await OnUploadCommandAsync());
            BuildCommand = new DelegateCommand(OnBuildCommand);
            ChangeIpCommand = new DelegateCommand(OnChangeIpCommand);

            NameList = new List<string>();
            UiControls = new UiPropertiesModel();

            _eventAggregator.GetEvent<UpdateControlsEvent>().Subscribe(OnUpdateControls);
            _eventAggregator.GetEvent<UpdateBrowserEvent>().Subscribe(OnUpdateBrowser);
            _eventAggregator.GetEvent<UpdateStatusEvent>().Subscribe(OnUpdateStatus);

            InitializeProgramAsync();
        }

        public async void InitializeProgramAsync()
        {
            SearchPhrase = _config.GetSearchPhrase();
            FilePath = _config.GetFilePath();
            NameList = await _buttonService.ExecuteUploadButtonAsync(FilePath, true); //need to be loaded before PhraseNo
            PhraseNo = _config.GetPhraseNo();
            _fileService.RemoveOldLogs();
        }

        private void OnUpdateControls(UiPropertiesModel obj)
        {
            UiControls = obj;
        }

        private void OnUpdateStatus(StatusPropertiesModel obj)
        {
            StatusControls = obj;
        }

        private void OnUpdateBrowser(BrowserPropertiesModel obj)
        {
            BrowserControls = obj;
        }

        private void OnChangeIpCommand(object obj)
        {
            if (UiButtonsEnabled)
                _buttonService.ExecuteChangeIpButtonAsync();
        }

        public async Task OnStartCommandAsync()
        {
            if (UiButtonsEnabled)
                await _buttonService.ExecuteStartButtonAsync(NameList, WebBrowser, SearchPhrase, Paused);
        }

        public void OnBuildCommand(object obj)
        {
            if (UiButtonsEnabled)
            {
                SearchPhrase = ShowUploadDialog(viewModel => _dialogService.ShowDialog<PhraseView>(this, viewModel));
                _buttonService.ExecuteBuildButton(SearchPhrase);
            }
        }

        public async Task OnUploadCommandAsync()
        {
            bool? deleteResults = false;

            if (UiButtonsEnabled)
            {
                FilePath = GetFilePath();
                if(!string.IsNullOrEmpty(FilePath))
                    NameList = await _buttonService.ExecuteUploadButtonAsync(FilePath, false);
                if (NameList.Count > 0)
                {
                    deleteResults = ShowDeleteDialog(viewModel => _dialogService.ShowDialog<DeleteView>(this, viewModel));
                }
                if (deleteResults == true)
                    _fileService.DeleteResultsFile();
            }

        }

        public void OnPauseCommand(object obj)
        {
            if (PauseBtnEnabled)
                _buttonService.ExecutePauseButton(Paused);
        }

        public void OnStopCommand(object obj)
        {
            bool? deleteResults = false;

            if (StopBtnEnabled)
                _buttonService.ExecuteStopButton();
            deleteResults = ShowDeleteDialog(viewModel => _dialogService.ShowDialog<DeleteView>(this, viewModel));
            if (deleteResults == true)
                _fileService.DeleteResultsFile();
        }

        public void OnClickerChangeCommand(object obj)
        {
            if (UiButtonsEnabled)
                _buttonService.ExecuteClickerChangeButton();
        }

        public async Task OnConnectionChangeCommandAsync()
        {
            if (UiButtonsEnabled)
                await _buttonService.ExecuteConnectionButtonAsync(WebBrowser, Paused);
        }

        public string GetFilePath()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = "This Is The Title",
                Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            settings.InitialDirectory = (!string.IsNullOrEmpty(FilePath) && !string.IsNullOrWhiteSpace(FilePath)) ? FilePath : Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            bool? success = _dialogService.ShowOpenFileDialog(this, settings);

            return success == true ? settings.FileName : string.Empty;
        }

        public string ShowUploadDialog(Func<PhraseViewModel, bool?> showDialog)
        {
            var dialogViewModel = new PhraseViewModel(SearchPhrase);

            bool? success = showDialog(dialogViewModel);

            return success == true ? dialogViewModel.Text : string.Empty;
        }

        public bool? ShowDeleteDialog(Func<DeleteViewModel, bool?> showDialog)
        {
            var dialogViewModel = new DeleteViewModel();

            bool? success = showDialog(dialogViewModel);

            return success;
        }

        public void ProgressBarTick()
        {
            if (NameList.Count > 0)
            {
                UpdateStatusBar = PhraseNo * 100 / NameList.Count;
                ProgressDisplay = PhraseNo + " / " + NameList.Count;
            }
            else if (NameList.Count == 0 || NameList == null)
            {
                UpdateStatusBar = 0;
                ProgressDisplay = "0 / 0";
            }

        }

        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand ConnectionChangeCommand { get; private set; }
        public ICommand ClickerChangeCommand { get; private set; }
        public IAsyncCommand UploadCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }
        public ICommand ChangeIpCommand { get; private set; }

        public string FilePath { get; set; } //path of uploaded file
        public string SearchPhrase { get; set; } //phrase built in dialog window

        private List<string> _nameList;
        public List<string> NameList //list of phrases loaded from the file
        {
            get => _nameList;
            set
            {
                _nameList = value;
                ProgressBarTick();
            }
        }

        private int _phraseNo;
        public int PhraseNo //what phraseno is currently processed
        {
            get => _phraseNo;
            set
            {
                _phraseNo = value;
                ProgressBarTick();
            }
        }

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

        private BrowserPropertiesModel _browserControls;
        public BrowserPropertiesModel BrowserControls
        {
            get => _browserControls;
            set
            {
                _browserControls = value;
                CurWindowState = _browserControls.WindowState;
                IsBrowserFocused = _browserControls.IsBrowserFocused;
                IsOnTop = _browserControls.IsOnTop;
                ResizeMode = _browserControls.ResizeMode;
                OnPropertyChanged();
            }
        }

        private StatusPropertiesModel _statusControls;
        public StatusPropertiesModel StatusControls
        {
            get => _statusControls;
            set
            {
                _statusControls = value;
                Connection = _statusControls.Connection;
                Status = _statusControls.Status;
                Clicker = _statusControls.Clicker;
                PhraseNo = _statusControls.PhraseNo;
                OnPropertyChanged();
            }
        }

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

        private bool _isOnTop;
        public bool IsOnTop
        {
            get => _isOnTop;
            set
            {
                _isOnTop = value;
                OnPropertyChanged();
            }
        }

        private ResizeMode _resizeMode;
        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set
            {
                _resizeMode = value;
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

        private string _connection;
        public string Connection
        {
            get => _connection;
            set
            {
                _connection = value;
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

        private string _progressDisplay;
        public string ProgressDisplay
        {
            get
            {
                return _progressDisplay;
            }
            set
            {
                _progressDisplay = value;
                OnPropertyChanged();
            }
        }

        private int _updateStatusBar;
        public int UpdateStatusBar
        {
            get
            {
                return _updateStatusBar;
            }
            set
            {
                _updateStatusBar = value;
                OnPropertyChanged();
            }
        }
    }
}
