using System.Threading.Tasks;
using CefSharp.Wpf;

namespace G_Hoover.Services.Scrap
{
    public interface IScrapService
    {
        Task<bool> EnterPhraseAsync(bool clickerInput, IWpfWebBrowser webBrowser, string phrase);
        Task<bool> CliskSearchBtnAsync(bool clickerInput, IWpfWebBrowser webBrowser);
        Task<bool> CheckForRecaptchaAsync(IWpfWebBrowser webBrowser);
    }
}