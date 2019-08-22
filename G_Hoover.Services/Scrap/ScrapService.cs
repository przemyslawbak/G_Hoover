using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Services.Input;

namespace G_Hoover.Services.Scrap
{
    public class ScrapService : IScrapService
    {
        IInputService _inputService;

        public ScrapService(IInputService inputService)
        {
            _inputService = inputService;
        }

        public async Task ClickSendResultAsync(bool clickerInput, string audioResult)
        {
            if (!clickerInput)
            {
                //TODO: find the way to do it with JS
            }
            else
            {
                await _inputService.ClickSendResultInputAsync();
            }
        }

        public async Task ClickAudioChallangeIconAsync(bool clickerInput)
        {
            if (!clickerInput)
            {
                //TODO: find the way to do it with JS
            }
            else
            {
                await _inputService.ClickAudioChallangeInputAsync();
            }
        }

        public async Task ClickCheckboxIcon(bool clickerInput)
        {
            if (!clickerInput)
            {
                //TODO: find the way to do it with JS
            }
            else
            {
                await _inputService.ClickCheckboxInputAsync();
            }
        }

        public async Task ClickNewAudioChallengeAsync(bool clickerInput, bool inputCorrection)
        {
            if (!clickerInput)
            {
                //TODO: find the way to do it with JS
            }
            else
            {
                await _inputService.ClickNewAudioChallengeInputAsync(inputCorrection);
            }
        }

        public async Task ClickPlayIconAsync(bool clickerInput, bool inputCorrection)
        {
            if (!clickerInput)
            {
                //TODO: find the way to do it with JS
            }
            else
            {
                await _inputService.ClickPlayInputAsync(inputCorrection);
            }
        }

        public async Task ClickTextBoxAsync(bool clickerInput)
        {
            if (!clickerInput)
            {
                //TODO: find the way to do it with JS
            }
            else
            {
                await _inputService.ClickTextBoxInputAsync();
            }
        }

        public async Task EnterResultAsync(bool clickerInput, string audioResult)
        {
            if (!clickerInput)
            {
                //TODO: find the way to do it with JS
            }
            else
            {
                await _inputService.EnterResulInputAsync(audioResult);
            }
        }

        public async Task CliskSearchBtnAsync(bool clickerInput, IWpfWebBrowser webBrowser)
        {
            try
            {
                if (!clickerInput)
                {
                    await webBrowser.EvaluateScriptAsync(@"
                           var arr = document.getElementsByTagName('input')[3].click();
                           ");
                }
                else
                {
                    //TODO: input clicks -> cancellation token?
                }
            }
            catch (Exception e)
            {
                //log
            }
        }

        public async Task EnterPhraseAsync(bool clickerInput, IWpfWebBrowser webBrowser, string phrase)
        {
            try
            {
                if (!clickerInput)
                {
                    await webBrowser.EvaluateScriptAsync(@"
                           var arr = document.getElementsByTagName('input')[2].value = '" + phrase + @"';
                           ");
                }
                else
                {
                    //TODO: input clicks -> cancellation token?
                }
            }
            catch (Exception e)
            {
                //log
            }
        }

        public async Task<string> GetHeaderAsync(IWpfWebBrowser webBrowser)
        {
            string header = "";

            Task<JavascriptResponse> findHeader = webBrowser.EvaluateScriptAsync(@"
			(function() {
				var results = document.getElementsByClassName('srg')[0];
                    var result = results.getElementsByClassName('g')[0];
                    var header = result.getElementsByTagName('h3')[0];
                    return header.innerHTML;
				}
			)();");

            await findHeader.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    var EvaluateJavaScriptResult = response.Success ? (response.Result ?? "null") : response.Message;
                    header = Convert.ToString(response.Result);
                }
            }
            );

            return header;
        }

        public async Task<string> GetUrlAsync(IWpfWebBrowser webBrowser)
        {
            string url = "";

            Task<JavascriptResponse> findUrl = webBrowser.EvaluateScriptAsync(@"
			(function() {
				var results = document.getElementsByClassName('srg')[0];
                    var result = results.getElementsByClassName('g')[0];
                    var url = result.getElementsByTagName('cite')[0];
                    return url.innerHTML;
				}
			)();");

            await findUrl.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    var EvaluateJavaScriptResult = response.Success ? (response.Result ?? "null") : response.Message;
                    url = Convert.ToString(response.Result);
                }
            }
            );

            return url;
        }

        public async Task TurnOffAlertsAsync(IWpfWebBrowser webBrowser)
        {
            await webBrowser.EvaluateScriptAsync(@"
			window.alert = function(){}
			");
        }

        public async Task<bool> CheckForRecaptchaAsync(IWpfWebBrowser webBrowser)
        {
            bool isCaptcha = false;

            Task<JavascriptResponse> verifyReCaptcha = webBrowser.EvaluateScriptAsync(@"
			(function() {
				const arr = document.getElementsByTagName('a');
				for (i = 0; i < arr.length; ++i) {
					item = arr[i];
					if (document.getElementById('infoDiv'))
						{ return true; }
					}
				}
			)();");

            await verifyReCaptcha.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    var EvaluateJavaScriptResult = response.Success ? (response.Result ?? "null") : response.Message;
                    isCaptcha = Convert.ToBoolean(response.Result);
                }
            }
            );

            return isCaptcha;
        }

        /*
        public async Task<int> FindIndex(IWpfWebBrowser webBrowser)
        {
            int index = 0;

            Task<JavascriptResponse> verifyWarning = webBrowser.EvaluateScriptAsync(@"
			(function() {
				const arr = document.getElementsByTagName('body')[1].innerHTML;
                        { return arr.indexOf('Your computer or network may be'); }
				}
			)();");

            await verifyWarning.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    var EvaluateJavaScriptResult = response.Success ? (response.Result ?? "null") : response.Message;
                    index = Convert.ToInt16(response.Result);
                }
            }
            );

            return index;
        }

        public async Task<bool> CheckForWarning(IWpfWebBrowser webBrowser)
        {
            bool isWarning = false;

            Task<JavascriptResponse> verifyWarning = webBrowser.EvaluateScriptAsync(@"
			(function() {
                    var result = document.getElementsByTagName('html');
                    for (i = 0; i < arr.length; ++i) {
					item = arr[i].innerHTML;
                    if (item.indexOf('Our systems have detected') > -1)
                        { return true; }
					}
			})();");

            await verifyWarning.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    var EvaluateJavaScriptResult = response.Success ? (response.Result ?? "null") : response.Message;
                    isWarning = Convert.ToBoolean(response.Result);
                }
            }
            );

            return isWarning;
        }
        */
    }
}
