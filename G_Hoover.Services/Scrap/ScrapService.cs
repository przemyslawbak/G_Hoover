using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Services.Input;
using Params_Logger;

namespace G_Hoover.Services.Scrap
{
    /// <summary>
    /// service class that collects or inputs data into a browser
    /// </summary>
    public class ScrapService : IScrapService
    {
        private readonly IInputService _inputService;
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();

        public ScrapService(IInputService inputService)
        {
            _inputService = inputService;
        }

        /// <summary>
        /// clicks html document object in a way depending on clickerInput
        /// </summary>
        /// <param name="clickerInput">bool if clicker enabled</param>
        public async Task ClickAudioChallangeIconAsync(bool clickerInput)
        {
            _log.Called(clickerInput);

            try
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
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// clicks html document object in a way depending on clickerInput
        /// </summary>
        /// <param name="clickerInput">bool if clicker enabled</param>
        public async Task ClickCheckboxIcon(bool clickerInput)
        {
            _log.Called(clickerInput);

            try
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
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// clicks html document object in a way depending on clickerInput
        /// </summary>
        /// <param name="clickerInput">bool if clicker enabled</param>
        /// <param name="inputCorrection">bool if correction enabled</param>
        public async Task ClickNewAudioChallengeAsync(bool clickerInput, bool inputCorrection)
        {
            _log.Called(clickerInput, inputCorrection);

            try
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
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// clicks html document object in a way depending on clickerInput
        /// </summary>
        /// <param name="clickerInput">bool if clicker enabled</param>
        /// <param name="inputCorrection">bool if correction enabled</param>
        public async Task ClickPlayIconAsync(bool clickerInput, bool inputCorrection)
        {
            _log.Called(clickerInput, inputCorrection);

            try
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
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// clicks html document object in a way depending on clickerInput
        /// </summary>
        /// <param name="clickerInput">bool if clicker enabled</param>
        public async Task ClickTextBoxAsync(bool clickerInput)
        {
            _log.Called(clickerInput);

            try
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
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// enters audio result in a way depending on clickerInput
        /// </summary>
        /// <param name="clickerInput">bool if clicker enabled</param>
        /// <param name="audioResult">string result to be entered</param>
        public async Task EnterResultAsync(bool clickerInput, string audioResult)
        {
            _log.Called(clickerInput, audioResult);

            try
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
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// clicks html document object in a way depending on clickerInput
        /// </summary>
        /// <param name="clickerInput">bool if clicker enabled</param>
        /// <param name="webBrowser">cefsharp browser interface</param>
        public async Task ClickSearchBtnAsync(bool clickerInput, IWpfWebBrowser webBrowser)
        {
            _log.Called(clickerInput, string.Empty);

            try
            {
                if (!clickerInput)
                {
                    await webBrowser.EvaluateScriptAsync(@"
                           var arr = document.getElementsByTagName('input')[3].click();
                           ");
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// enters phrase in a way depending on clickerInput
        /// </summary>
        /// <param name="clickerInput">bool if clicker enabled</param>
        /// <param name="webBrowser">cafsharp browser interface</param>
        /// <param name="phrase">built search phrase</param>
        public async Task EnterPhraseAsync(bool clickerInput, IWpfWebBrowser webBrowser, string phrase)
        {
            _log.Called(clickerInput, string.Empty, phrase);

            try
            {
                if (!clickerInput)
                {
                    phrase = ValidatePhrase(phrase);

                    await webBrowser.EvaluateScriptAsync(@"
                           var arr = document.getElementsByTagName('input')[2].value = '" + phrase + @"';
                           ");
                }
                else
                {
                    await _inputService.ClickSearchBarInputAsync();

                    await _inputService.EnterPhraseInputAsync(phrase);
                }
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// searches for header data in html document
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        /// <returns>string tag data</returns>
        public async Task<string> GetHeaderAsync(IWpfWebBrowser webBrowser)
        {
            _log.Called(string.Empty);

            try
            {
                string header = "";

                Task<JavascriptResponse> findHeader = webBrowser.EvaluateScriptAsync(@"
			(function() {
				var results = document.getElementsByClassName('srg')[0];
                    var result = results.getElementsByClassName('g')[0];
                    var header = result.getElementsByTagName('h3')[0];
                    var result = header.getElementsByTagName('div')[0];
                    return result.innerHTML;
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
            catch (Exception e)
            {
                _log.Error(e.Message);

                return string.Empty;
            }
        }

        /// <summary>
        /// searches for URL data in html document
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        /// <returns>string tag data</returns>
        public async Task<string> GetUrlAsync(IWpfWebBrowser webBrowser)
        {
            _log.Called(string.Empty);

            try
            {
                string url = "";

                Task<JavascriptResponse> findUrl = webBrowser.EvaluateScriptAsync(@"
			(function() {
				var results = document.getElementsByClassName('srg')[0];
                    var result = results.getElementsByClassName('g')[0];
                    var div = result.getElementsByTagName('div')[0];
                    return div.innerHTML;
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

                url = SplitUrl(url);

                return url;
            }
            catch (Exception e)
            {
                _log.Error(e.Message);

                return string.Empty;
            }
        }

        /// <summary>
        /// corrects string phrase
        /// </summary>
        /// <param name="phrase">string phrase</param>
        /// <returns>corrected phrase</returns>
        private string ValidatePhrase(string phrase)
        {
            if (phrase.Contains("'"))
            {
                phrase = phrase.Replace("'", "\\'");
            }

            return phrase;
        }

        /// <summary>
        /// splits scrapped URL
        /// </summary>
        /// <param name="url">scrapped URL with tag</param>
        /// <returns>splitted URL</returns>
        private string SplitUrl(string url)
        {
            return url.Split(new[] { "<a href=\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        /// <summary>
        /// switches off JS alerts
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        public async Task TurnOffAlertsAsync(IWpfWebBrowser webBrowser)
        {
            await webBrowser.EvaluateScriptAsync(@"
			window.alert = function(){}
			");
        }

        /// <summary>
        /// checks if in the html document is captcha
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        public async Task<bool> CheckForRecaptchaAsync(IWpfWebBrowser webBrowser)
        {
            _log.Called(string.Empty);

            try
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
            catch (Exception e)
            {
                _log.Error(e.Message);

                return true;
            }
        }
    }
}
