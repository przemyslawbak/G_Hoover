using G_Hoover.Desktop.Commands;
using MvvmDialogs;
using Params_Logger;
using System;
using System.Windows.Input;

namespace G_Hoover.Desktop.ViewModels
{
    /// <summary>
    /// dialog viewmodel responsible for building search phrase
    /// </summary>
    public class PhraseViewModel : ViewModelBase, IModalDialogViewModel
    {
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();

        public PhraseViewModel(string searchPhrase)
        {
            OkCommand = new DelegateCommand(Ok);

            SplitPhrase(searchPhrase);
        }

        /// <summary>
        /// splitting phrase into `Before` and `After`
        /// </summary>
        /// <param name="searchPhrase">complete phrase passed to the VM</param>
        public void SplitPhrase(string searchPhrase)
        {
            if (searchPhrase.Contains("<name>"))
            {
                After = searchPhrase.Split(new[] { "<name>" }, StringSplitOptions.None)[1];
                Before = searchPhrase.Split(new[] { "<name>" }, StringSplitOptions.None)[0];
            }
        }

        public ICommand OkCommand { get; } //on OK click

        private bool? _dialogResult;
        public bool? DialogResult //prop takes bool dialog result for OK button = true
        {
            get => _dialogResult;
            set
            {
                _dialogResult = value;
                OnPropertyChanged();
                _log.Prop(_dialogResult);
            }
        }

        /// <summary>
        /// on OK btn click called, makes Text prop valid
        /// </summary>
        private void Ok(object obj)
        {
            _log.Called(string.Empty);

            try
            {
                string text = ValidateText(Text);

                DialogResult = !string.IsNullOrEmpty(text) ? true : false;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// makes text string valid
        /// </summary>
        /// <param name="text">valid text string</param>
        /// <returns></returns>
        private string ValidateText(string text)
        {
            if (text.Contains("\""))
            {
                text = text.Replace("\"", "\\\"");
            }

            return text;
        }

        public string Name { get; } = "<name>"; //readonly prop for <name> inside of searched phrase

        private string _after;
        public string After //text after <name>
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
        public string Before //text before <name>
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
        public string Text //complete phrase with Before, Name, After
        {
            get => Before + Name + After;
        }

        private bool ValidateInput //checks if After or Before contains <name>
        {
            get
            {
                return !After.Contains("<name>") && !Before.Contains("<name>");
            }
        }
    }
}
