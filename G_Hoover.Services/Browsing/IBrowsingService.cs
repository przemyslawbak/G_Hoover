using System.Collections.Generic;
using System.Threading.Tasks;
using CefSharp.Wpf;

namespace G_Hoover.Services.Browsing
{
    public interface IBrowsingService
    {
        Task CollectDataAsync(List<string> nameList, IWpfWebBrowser webBrowser);
        void CancelCollectData();
    }
}