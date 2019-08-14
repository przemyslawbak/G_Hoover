using G_Hoover.Desktop.Commands;
using MvvmDialogs;
using System;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public class PhraseViewModel : ViewModelBase, IModalDialogViewModel
    {
        private bool? dialogResult;

        public PhraseViewModel(string searchPhrase)
        {
            OkCommand = new DelegateCommand(Ok);

            Before = "";
            After = "";
            SplitPhrase(searchPhrase);
        }

        public void SplitPhrase(string searchPhrase)
        {
            if (searchPhrase.Contains("<name>"))
            {
                After = searchPhrase.Split(new[] { "<name>" }, StringSplitOptions.None)[1];
                Before = searchPhrase.Split(new[] { "<name>" }, StringSplitOptions.None)[0];
            }
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
                OnPropertyChanged("ValidateInput");
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
                OnPropertyChanged("ValidateInput");
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

        public bool ValidateInput
        {
            get
            {
                return !After.Contains("<name>") && !Before.Contains("<name>");
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

        public void Ok(object obj)
        {
            if (!string.IsNullOrEmpty(Text))
            {
                DialogResult = true;
            }
        }
    }
}
