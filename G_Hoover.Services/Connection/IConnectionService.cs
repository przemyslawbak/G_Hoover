using System.Threading.Tasks;
using CefSharp.Wpf;

namespace G_Hoover.Services.Connection
{
    public interface IConnectionService
    {
        Task ConfigureBrowserTor(IWpfWebBrowser webBrowser);
        Task ConfigureBrowserDirect(IWpfWebBrowser webBrowser);
    }
}