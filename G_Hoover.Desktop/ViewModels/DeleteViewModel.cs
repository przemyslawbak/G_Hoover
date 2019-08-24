using G_Hoover.Desktop.Commands;
using MvvmDialogs;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public class DeleteViewModel : ViewModelBase, IModalDialogViewModel
    {
        public DeleteViewModel()
        {
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
            DialogResult = true;
        }
    }
}
