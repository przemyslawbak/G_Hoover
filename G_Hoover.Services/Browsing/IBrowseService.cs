using System.Collections.Generic;
using System.Threading.Tasks;
using CefSharp.Wpf;
using G_Hoover.Models;

namespace G_Hoover.Services.Browsing
{
    public interface IBrowseService
    {
        void CancelCollectData();
        Task CollectDataAsync(List<string> nameList, IWpfWebBrowser webBrowser, string searchPhrase);
        void UpdateSearchPhrase(string searchPhrase);
        void ChangeConnectionType(IWpfWebBrowser webBrowser);
        void SaveFilePath(string filePath);
    }
}