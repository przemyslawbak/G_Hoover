using G_Hoover.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace G_Hoover.ViewModels
{
    public class BrowserViewModel : ViewModelBase
    {
        public BrowserViewModel()
        {
            StartCommand = new DelegateCommand(OnStartCommand);
            StopCommand = new DelegateCommand(OnStopCommand);
            PauseCommand = new DelegateCommand(OnPauseCommand);
            ConnectionChangeCommand = new DelegateCommand(OnConnectionChangeCommand);
            ClickerChangeCommand = new DelegateCommand(OnClickerChangeCommand);

            InitializeBrowser();
        }

        private void InitializeBrowser()
        {
            throw new NotImplementedException();
        }

        private void OnClickerChangeCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnConnectionChangeCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnPauseCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnStopCommand(object obj)
        {
            throw new NotImplementedException();
        }

        private void OnStartCommand(object obj)
        {
            throw new NotImplementedException();
        }

        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PauseCommand { get; private set; }
        public ICommand ConnectionChangeCommand { get; private set; }
        public ICommand ClickerChangeCommand { get; private set; }

        public bool ClickerInput { get; set; } //if click by input simulation
        public string CompletePhrase { get; set; } //search phrase builded
        public bool SearchViaTor { get; set; } //if searching when using Tor network
        //public ChromiumWebBrowser

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
    }
}
