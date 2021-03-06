﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using CefSharp;
using CefSharp.Wpf;
using Params_Logger;
using Tor;
using Tor.Config;

namespace G_Hoover.Services.Connection
{
    /// <summary>
    /// service class that establishes Tor or direct connection for the browser and changes IP
    /// </summary>
    public class ConnectionService : IConnectionService
    {
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();

        /// <summary>
        /// setting up browser for using direct connection
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        public void ConfigureBrowserDirect(IWpfWebBrowser webBrowser)
        {
            _log.Called(string.Empty);

            try
            {
                Cef.UIThreadTaskFactory.StartNew(delegate
                {
                    var rc = webBrowser.GetBrowser().GetHost().RequestContext;
                    var v = new Dictionary<string, object>();
                    v["mode"] = "direct";
                    v["server"] = "";
                    string error;
                    bool success = rc.SetPreference("proxy", v, out error);
                });
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// setting up browser for using Tor connection
        /// </summary>
        /// <param name="webBrowser">cefsharp browser interface</param>
        public void ConfigureBrowserTor(IWpfWebBrowser webBrowser)
        {
            _log.Called(string.Empty);

            try
            {
                InitializeTor();

                Cef.UIThreadTaskFactory.StartNew(delegate
                {
                    var rc = webBrowser.GetBrowser().GetHost().RequestContext;
                    var v = new Dictionary<string, object>();
                    v["mode"] = "fixed_servers";
                    v["server"] = "socks5://127.0.0.1:9050";
                    string error;
                    bool success = rc.SetPreference("proxy", v, out error);
                });
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }

        /// <summary>
        /// initializes Tor browser connection
        /// </summary>
        public void InitializeTor()
        {
            _log.Called();

            try
            {
                Cef.GetGlobalCookieManager().DeleteCookies("", "");

                Process[] previous = Process.GetProcessesByName("tor");
                if (previous != null && previous.Length > 0)
                {
                    foreach (Process process in previous)
                        process.Kill();
                }

                ClientCreateParams createParams = new ClientCreateParams();
                createParams.ConfigurationFile = "";
                createParams.DefaultConfigurationFile = "";
                createParams.ControlPassword = "";
                createParams.ControlPort = 9051;
                createParams.Path = @"Tor\Tor\tor.exe";
                createParams.SetConfig(ConfigurationNames.AvoidDiskWrites, true);
                createParams.SetConfig(ConfigurationNames.GeoIPFile, System.IO.Path.Combine(Environment.CurrentDirectory, @"Tor\Data\Tor\geoip"));
                createParams.SetConfig(ConfigurationNames.GeoIPv6File, System.IO.Path.Combine(Environment.CurrentDirectory, @"Tor\Data\Tor\geoip6"));

                Client client = Client.Create(createParams);
            }
            catch (Exception e)
            {
                _log.Error(e.Message);
            }
        }
    }
}
