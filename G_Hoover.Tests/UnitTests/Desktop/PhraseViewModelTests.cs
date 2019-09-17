using G_Hoover.Desktop.ViewModels;
using Xunit;

namespace G_Hoover.Tests.UnitTests.Desktop
{
    public class PhraseViewModelTests
    {
        private readonly string _searchPhrase = "testBefore <name> testAfter";
        private readonly PhraseViewModel _vm;

        public PhraseViewModelTests()
        {
            _vm = new PhraseViewModel(_searchPhrase);
        }

        [Fact]
        public void SplitPhrase_CalledWithParameter_UpdatesPropertiesAfterAndBefore()
        {
            _vm.After = "";
            _vm.Before = "";

            _vm.SplitPhrase(_searchPhrase);

            Assert.Equal("testBefore ", _vm.Before);
            Assert.Equal(" testAfter", _vm.After);
        }

        [Theory]
        [InlineData("", "", true)]
        [InlineData(null, null, true)]
        [InlineData("somethingBefore", "somethingAfter", true)]
        public void OkExecute_Called_ReturnsTrueAndUpdatesTextProp(string beforeText, string afterText, bool expected)
        {
            _vm.DialogResult = null;
            _vm.Before = beforeText;
            _vm.After = afterText;

            _vm.OkCommand.Execute(null);

            Assert.Equal(expected, _vm.DialogResult);
            Assert.Equal(_vm.Text, beforeText + "<name>" + afterText);
        }

        [Fact]
        public void OkExecute_Called_CorrectsQuotes()
        {
            _vm.Before = "\"dupajana";
            _vm.After = "\"janadupa";

            _vm.OkCommand.Execute(null);

            Assert.Equal("\"dupajana" + _vm.Name + "\"janadupa", _vm.Text);
        }
    }
}
