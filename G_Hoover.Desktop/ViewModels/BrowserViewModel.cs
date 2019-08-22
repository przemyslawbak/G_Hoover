using CefSharp.Wpf;
using G_Hoover.Desktop.Commands;
using G_Hoover.Desktop.Views;
using G_Hoover.Events;
using G_Hoover.Models;
using G_Hoover.Services.Buttons;
using G_Hoover.Services.Config;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using NLog;
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
        private readonly IControlsService _controlsService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IButtonsService _buttonService;
        private readonly IAppConfig _config;
        private readonly Logger _logger;

        public BrowserViewModel(IFileService fileService,
            IDialogService dialogService,
            IMessageService messageService,
            IControlsService controlsService,
            IEventAggregator eventAggregator,
            IButtonsService buttonService,
            IAppConfig config)
        {
            _dialogService = dialogService;
            _fileService = fileService;
            _controlsService = controlsService;
            _buttonService = buttonService;
            _eventAggregator = eventAggregator;
            _config = config;
            _logger = LogManager.GetCurrentClassLogger();

            StartCommand = new AsyncCommand(async () => await OnStartCommandAsync());
            StopCommand = new DelegateCommand(OnStopCommand);
            PauseCommand = new DelegateCommand(OnPauseCommand);
            ConnectionChangeCommand = new AsyncCommand(async () => await OnConnectionChangeCommandAsync());
            ClickerChangeCommand = new DelegateCommand(OnClickerChangeCommand);
            UploadCommand = new AsyncCommand(async () => await OnUploadCommandAsync());
            BuildCommand = new DelegateCommand(OnBuildCommand);

            NameList = new List<string>();
            UiControls = new UiPropertiesModel();

            _eventAggregator.GetEvent<UpdateControlsEvent>().Subscribe(OnUpdateControls);
            _eventAggregator.GetEvent<UpdateBrowserEvent>().Subscribe(OnUpdateBrowser);

            InitializeProgramAsync();
        }

        public async void InitializeProgramAsync()
        {
            SearchPhrase = _config.GetSearchPhrase();
            FilePath = _config.GetFilePath();
            PhraseNo = _config.GetPhraseNo();
            NameList = await _buttonService.ExecuteUploadButtonAsync(FilePath);
            _fileService.RemoveOldLogs();
            _controlsService.GetStoppedConfiguration();
        }

        private void OnUpdateControls(UiPropertiesModel obj)
        {
            UiControls = obj;
        }

        private void OnUpdateBrowser(BrowserPropertiesModel obj)
        {
            BrowserControls = obj;
        }

        public async Task OnStartCommandAsync()
        {
            await _buttonService.ExecuteStartButtonAsync(NameList, WebBrowser, SearchPhrase, Paused);
        }

        public void OnBuildCommand(object obj)
        {
            SearchPhrase = ShowDialog(viewModel => _dialogService.ShowDialog<PhraseView>(this, viewModel));

            _buttonService.ExecuteBuildButton(SearchPhrase);
        }

        public async Task OnUploadCommandAsync()
        {
            FilePath = GetFilePath();

            NameList = await _buttonService.ExecuteUploadButtonAsync(FilePath);
        }

        public void OnPauseCommand(object obj)
        {
            _buttonService.ExecutePauseButton(Paused);
        }

        public void OnStopCommand(object obj)
        {
            _buttonService.ExecuteStopButton();
        }

        public void OnClickerChangeCommand(object obj)
        {
            throw new NotImplementedException();
        }

        public async Task OnConnectionChangeCommandAsync()
        {
            await _buttonService.ExecuteConnectionButtonAsync(WebBrowser, Paused);
        }

        public string GetFilePath()
        {
            var settings = new OpenFileDialogSettings
            {
                Title = "This Is The Title",
                Filter = "Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (!string.IsNullOrEmpty(FilePath) && !string.IsNullOrWhiteSpace(FilePath))
            {
                settings.InitialDirectory = FilePath;
            }
            else
            {
                settings.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }

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

        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand ConnectionChangeCommand { get; private set; }
        public ICommand ClickerChangeCommand { get; private set; }
        public IAsyncCommand UploadCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }
        public List<string> NameList { get; set; } //list of phrases loaded from the file
        public string FilePath { get; set; } //path of uploaded file
        public string SearchPhrase { get; set; } //phrase built in dialog window
        public int PhraseNo { get; set; }

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
