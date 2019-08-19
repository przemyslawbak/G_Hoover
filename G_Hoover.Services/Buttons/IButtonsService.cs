using CefSharp.Wpf;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G_Hoover.Services.Buttons
{
    public interface IButtonsService
    {
        void ExecuteStopButton();
        void ExecutePauseButton(bool paused);
        Task ExecuteStartButtonAsync(List<string> nameList, IWpfWebBrowser webBrowser, string searchPhrase);
        Task<List<string>> ExecuteUploadButtonAsync(string filePath);
        Task ExecuteBuildButtonAsync(string searchPhrase);
        void ExecuteConnectionButton(IWpfWebBrowser webBrowser);
    }
}