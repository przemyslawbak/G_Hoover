using G_Hoover.Services.Logging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace G_Hoover.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ILogService _log;

        public App()
        {

        }

        public App(ILogService log) : this()
        {
            _log = log;
        }

        async Task App_DispatcherUnhandledExceptionAsync(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            _log.Error("App.xaml.cs exception: " + e.Exception.Message);

            await _log.SaveAllLinesAsync();

            e.Handled = true;
        }
    }
}
