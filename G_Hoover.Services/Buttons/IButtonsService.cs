using CefSharp.Wpf;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace G_Hoover.Services.Buttons
{
    public interface IButtonsService
    {
        void ExecuteStopButton();
        void ExecutePauseButton(bool paused);
        Task ExecuteStartButtonAsync(List<string> nameList, IWpfWebBrowser webBrowser, string searchPhrase, bool paused);
        Task<List<string>> ExecuteUploadButtonAsync(string filePath);
        void ExecuteBuildButton(string searchPhrase);
        Task ExecuteConnectionButtonAsync(IWpfWebBrowser webBrowser, bool paused);
        void ExecuteClickerChangeButton();
    }
}