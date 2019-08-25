using G_Hoover.Desktop.Commands;
using G_Hoover.Services.Logging;
using MvvmDialogs;
using System;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public class DeleteViewModel : ViewModelBase, IModalDialogViewModel
    {
        private readonly ILogService _log;
        public DeleteViewModel(ILogService log)
        {
            _log = log;

            OkCommand = new DelegateCommand(Ok);
        }

        public ICommand OkCommand { get; }

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set
            {
                _dialogResult = value;
                OnPropertyChanged();
            }
        }

        public void Ok(object obj)
        {
            _log.Called();

            try
            {
                DialogResult = true;

                _log.Ended(DialogResult);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }
    }
}
