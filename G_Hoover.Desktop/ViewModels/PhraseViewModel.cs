using G_Hoover.Desktop.Commands;
using G_Hoover.Services.Logging;
using MvvmDialogs;
using System;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    public class PhraseViewModel : ViewModelBase, IModalDialogViewModel
    {
        private readonly ILogService _log;
        public PhraseViewModel(string searchPhrase, ILogService log)
        {
            _log = log;

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

        public ICommand OkCommand { get; }

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
            _log.Called(obj.ToString());

            try
            {
                DialogResult = !string.IsNullOrEmpty(Text) ? true : false;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
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
    }
}
