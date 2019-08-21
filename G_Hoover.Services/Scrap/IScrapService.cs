using System.Threading.Tasks;
using CefSharp.Wpf;

namespace G_Hoover.Services.Scrap
{
    public interface IScrapService
    {
        Task EnterPhraseAsync(bool clickerInput, IWpfWebBrowser webBrowser, string phrase);
        Task CliskSearchBtnAsync(bool clickerInput, IWpfWebBrowser webBrowser);
        Task<bool> CheckForRecaptchaAsync(IWpfWebBrowser webBrowser);
        Task<string> GetHeaderAsync(IWpfWebBrowser webBrowser);
        Task<string> GetUrl(IWpfWebBrowser webBrowser);
        Task TurnOffAlerts(IWpfWebBrowser webBrowser);
        Task ClickCheckboxIcon(bool clickerInput);
        Task ClickAudioChallangeIcon(bool clickerInput);
        Task ClickPlayIcon(bool clickerInput);
    }
}