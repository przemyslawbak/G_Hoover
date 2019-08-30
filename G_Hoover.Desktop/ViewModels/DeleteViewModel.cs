using G_Hoover.Desktop.Commands;
using G_Hoover.Services.Logging;
using MvvmDialogs;
using System;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public class DeleteViewModel : ViewModelBase, IModalDialogViewModel
    {
        private readonly IParamsLogger _log;
        public DeleteViewModel(IParamsLogger log, int qty)
        {
            _log = log;
            HowManyResults = qty.ToString();

            OkCommand = new DelegateCommand(Ok);
        }

        public ICommand OkCommand { get; }

        private string _howManyResults;
        public string HowManyResults
        {
            get => "Total results: " + _howManyResults;
            private set
            {
                _howManyResults = value;
                OnPropertyChanged();
                _log.Prop(_howManyResults);
            }
        }

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set
            {
                _dialogResult = value;
                OnPropertyChanged();
                _log.Prop(_dialogResult);
            }
        }

        public void Ok(object obj)
        {
            _log.Called(string.Empty);

            try
            {
                DialogResult = true;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }
    }
}
