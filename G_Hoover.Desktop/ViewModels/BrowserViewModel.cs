﻿using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Commands;
using G_Hoover.Desktop.Commands;
using G_Hoover.Services.Files;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
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

        public BrowserViewModel(IFileService fileService, IDialogService dialogService)
        {
            _dialogService = dialogService;
            _fileService = fileService;

            StartCommand = new DelegateCommand(OnStartCommand);
            StopCommand = new DelegateCommand(OnStopCommand);
            PauseCommand = new DelegateCommand(OnPauseCommand);
            ConnectionChangeCommand = new DelegateCommand(OnConnectionChangeCommand);
            ClickerChangeCommand = new DelegateCommand(OnClickerChangeCommand);
            UploadCommand = new AsyncCommand(async () => await OnUploadCommandAsync());
            BuildCommand = new DelegateCommand(OnBuildCommand);

            InitializeBrowser();
        }

        public async void InitializeBrowser()
        {
            StoppedConfiguration();

            NameList = new List<string>();

            await Task.Delay(5000);
            Address = "https://www.google.com/";
        }

        public void StoppedConfiguration()
        {
            UiButtonsEnabled = true;
            StopBtnEnabled = false;
            PauseBtnEnabled = false;
        }

        public void StartedConfiguration()
        {
            UiButtonsEnabled = false;
            StopBtnEnabled = true;
            PauseBtnEnabled = true;
        }

        private void StartCrawling()
        {
            throw new NotImplementedException();
        }

        public void OnBuildCommand(object obj)
        {
            throw new NotImplementedException();
        }

        public async Task OnUploadCommandAsync()
        {
            NameList.Clear();
            FilePath = GetFilePath();

            try
            {
                if (!string.IsNullOrEmpty(FilePath))
                {
                    NameList = await _fileService.GetNewListFromFileAsync(FilePath);

                    if (NameList.Count > 0)
                    {
                        Message = _dialogService.ShowMessageBox(this,"Loaded successfully.");
                    }
                    else
                    {
                        throw new Exception("Empty file.");
                    }
                }
                else
                {
                    throw new Exception("Wrong path.");
                }
            }
            catch (Exception e)
            {
                Message = _dialogService.ShowMessageBox(this,"Unable to load data. " + e.Message);
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

        public void OnStartCommand(object obj)
        {
            StartedConfiguration();

            StartCrawling();
        }

        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand ConnectionChangeCommand { get; private set; }
        public ICommand ClickerChangeCommand { get; private set; }
        public IAsyncCommand UploadCommand { get; private set; }
        public ICommand BuildCommand { get; private set; }

        public bool ClickerInput { get; set; } //if click by input simulation
        public string CompletePhrase { get; set; } //search phrase builded
        public bool SearchViaTor { get; set; } //if searching when using Tor network
        public int PhraseNo { get; set; } //number of currently checked phrase
        public List<string> NameList { get; set; } //list of phrases loaded from the file
        public string FilePath { get; set; } //path of uploaded file
        public MessageBoxResult Message { get; set; } //error messages

        private bool pauseBtnEnabled;
        public bool PauseBtnEnabled
        {
            get
            {
                return pauseBtnEnabled;
            }
            set
            {
                pauseBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool stopBtnEnabled;
        public bool StopBtnEnabled
        {
            get
            {
                return stopBtnEnabled;
            }
            set
            {
                stopBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool uiButtonsEnabled;
        public bool UiButtonsEnabled
        {
            get
            {
                return uiButtonsEnabled;
            }
            set
            {
                uiButtonsEnabled = value;
                OnPropertyChanged();
            }
        }

        private string address;
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
                OnPropertyChanged();
            }
        }

        private IWpfWebBrowser webBrowser;
        public IWpfWebBrowser WebBrowser
        {
            get
            {
                return webBrowser;
            }
            set
            {
                webBrowser = value;
                OnPropertyChanged();
            }
        }

        private string network;
        public string Network
        {
            get
            {
                return network;
            }
            set
            {
                network = value;
                OnPropertyChanged();
            }
        }

        private string clicker;
        public string Clicker
        {
            get
            {
                return clicker;
            }
            set
            {
                clicker = value;
                OnPropertyChanged();
            }
        }

        private string status;
        public string Status
        {
            get
            {
                return status;
            }
            set
            {
                status = value;
                OnPropertyChanged();
            }
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
    }
}
