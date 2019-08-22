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
        Task<string> GetUrlAsync(IWpfWebBrowser webBrowser);
        Task TurnOffAlertsAsync(IWpfWebBrowser webBrowser);
        Task ClickCheckboxIcon(bool clickerInput);
        Task ClickAudioChallangeIconAsync(bool clickerInput);
        Task ClickPlayIconAsync(bool clickerInput, bool inputCorrection);
        Task ClickNewAudioChallengeAsync(bool clickerInput, bool inputCorrection);
        Task ClickTextBoxAsync(bool clickerInput);
        Task EnterResultAsync(bool clickerInput, string audioResult);
        Task ClickSendResultAsync(bool clickerInput, string audioResult);
    }
}