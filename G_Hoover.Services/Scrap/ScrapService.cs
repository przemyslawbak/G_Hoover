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

        public async Task<bool> CliskSearchBtnAsync(bool clickerInput, IWpfWebBrowser webBrowser)
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
                    //input clicks
                }

                return true;
            }
            catch (Exception e)
            {
                //log

                return false;
            }
        }

        public async Task<bool> EnterPhraseAsync(bool clickerInput, IWpfWebBrowser webBrowser, string phrase)
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
                    //input clicks
                }

                return true;
            }
            catch (Exception e)
            {
                //log

                return false;
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

        public async Task<string> GetUrl(IWpfWebBrowser webBrowser)
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
    }
}
