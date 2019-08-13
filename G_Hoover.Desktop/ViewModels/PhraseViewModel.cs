using G_Hoover.Desktop.Commands;
using MvvmDialogs;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public class PhraseViewModel : ViewModelBase, IModalDialogViewModel
    {
        private bool? dialogResult;

        public PhraseViewModel()
        {
            OkCommand = new DelegateCommand(Ok);
        }

        public string Name { get; set; } = "<name>";

        private string _after;
        public string After
        {
            get => _after;
            set
            {
                _after = value;
                OnPropertyChanged();
                _text = Before + "<name>" + After;
            }
        }

        private string _before;
        public string Before
        {
            get => _before;
            set
            {
                _before = value;
                OnPropertyChanged();
                _text = Before + "<name>" + After;
            }
        }

        private string _text;
        public string Text
        {
            get => Before + "<name>" + After;
            set
            {
                _text = value;
                OnPropertyChanged();
            }
        }

        public ICommand OkCommand { get; }

        public bool? DialogResult
        {
            get => dialogResult;
            private set
            {
                dialogResult = value;
                OnPropertyChanged();
            }
        }

        private void Ok(object obj)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                DialogResult = true;
            }
        }
    }
}
