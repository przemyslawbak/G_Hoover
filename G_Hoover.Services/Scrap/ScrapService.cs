using System;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using G_Hoover.Services.Input;
using G_Hoover.Services.Logging;

namespace G_Hoover.Services.Scrap
{
    public class ScrapService : IScrapService
    {
        private readonly IInputService _inputService;
        private readonly ILogService _log;

        public ScrapService(IInputService inputService, ILogService log)
        {
            _inputService = inputService;
            _log = log;
        }

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

        public async Task CliskSearchBtnAsync(bool clickerInput, IWpfWebBrowser webBrowser)
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

        private string ValidatePhrase(string phrase)
        {
            if (phrase.Contains("'"))
            {
                phrase = phrase.Replace("'", "\\'");
            }

            return phrase;
        }

        private string SplitUrl(string url)
        {
            return url.Split(new[] { "<a href=\"" }, StringSplitOptions.None)[1].Split('"')[0];
        }

        public async Task TurnOffAlertsAsync(IWpfWebBrowser webBrowser)
        {
            await webBrowser.EvaluateScriptAsync(@"
			window.alert = function(){}
			");
        }

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
