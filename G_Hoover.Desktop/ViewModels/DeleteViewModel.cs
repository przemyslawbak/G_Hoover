using G_Hoover.Desktop.Commands;
using MvvmDialogs;
using Params_Logger;
using System;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    /// <summary>
    /// dialog viewmodel responsible for deleting (or not) current results
    /// </summary>
    public class DeleteViewModel : ViewModelBase, IModalDialogViewModel
    {
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();
        public DeleteViewModel(int qty)
        {
            HowManyResults = qty.ToString();

            OkCommand = new DelegateCommand(Ok);
        }

        public ICommand OkCommand { get; } //on OK click

        private string _howManyResults;
        public string HowManyResults //prop takes qty of records in current result file
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
        public bool? DialogResult //prop takes bool dialog result for OK button = true
        {
            get => _dialogResult;
            private set
            {
                _dialogResult = value;
                OnPropertyChanged();
                _log.Prop(_dialogResult);
            }
        }

        /// <summary>
        /// on OK btn click called
        /// </summary>
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
