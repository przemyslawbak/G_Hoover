using System.Threading.Tasks;
using CefSharp.Wpf;

namespace G_Hoover.Services.Connection
{
    public interface IConnectionService
    {
        void ConfigureBrowserTor(IWpfWebBrowser webBrowser);
        void ConfigureBrowserDirect(IWpfWebBrowser webBrowser);
        void GetNewBrowsingIp(IWpfWebBrowser webBrowser);
    }
}