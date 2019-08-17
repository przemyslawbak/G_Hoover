using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;

namespace G_Hoover.Services.Scrap
{
    public class ScrapService : IScrapService
    {
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

        public async Task CliskSearchBtnAsync(bool clickerInput, IWpfWebBrowser webBrowser)
        {
            if (!clickerInput)
            {
                await webBrowser.EvaluateScriptAsync(@"
                           var arr = document.getElementsByTagName('input')[3].click();
                           ");
            }
            else
            {
                //input clicks
            }
        }

        public async Task EnterPhraseAsync(bool clickerInput, IWpfWebBrowser webBrowser, string phrase)
        {
            if (!clickerInput)
            {
                await webBrowser.EvaluateScriptAsync(@"
                           var arr = document.getElementsByTagName('input')[2].value = '" + phrase + @"';
                           ");
            }
            else
            {
                //input clicks
            }
        }
    }
}
