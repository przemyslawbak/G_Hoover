using G_Hoover.Desktop.ViewModels;
using Xunit;

namespace G_Hoover.Tests.UnitTests.Desktop
{
    public class DeleteViewModelTests
    {
        private readonly int _qty = 5;
        private readonly DeleteViewModel _vm;

        public DeleteViewModelTests()
        {
            _vm = new DeleteViewModel(_qty);
        }

        [Fact]
        public void OkExecute_Called_ReturnsTrue()
        {
            _vm.DialogResult = null;

            _vm.OkCommand.Execute(null);

            Assert.True(_vm.DialogResult);
        }

    }
}
