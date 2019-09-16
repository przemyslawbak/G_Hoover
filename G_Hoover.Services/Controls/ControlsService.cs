using System.Diagnostics;
using System.Windows;
using G_Hoover.Events;
using G_Hoover.Models;
using Params_Logger;
using Prism.Events;

namespace G_Hoover.Services.Controls
{
    /// <summary>
    /// service class used for setting up display and UI properties depending on browsing status
    /// </summary>
    public class ControlsService : IControlsService
    {
        private readonly IEventAggregator _eventAggregator;
        private static readonly ILogger _log = ParamsLogger.LogInstance.GetLogger();

        public ControlsService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        /// props for paused browsing, updates UpdateControlsEvent
        /// </summary>
        public void GetPausedConfiguration()
        {
            _log.Called();

            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = true,
                PleaseWaitVisible = false,
                UiButtonsEnabled = true,
                StopBtnEnabled = true,
                PauseBtnEnabled = true,
                Stopped = false,
                Init = false
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        /// <summary>
        /// props for started browsing, updates UpdateControlsEvent
        /// </summary>
        public void GetStartedConfiguration()
        {
            _log.Called();

            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = false,
                PleaseWaitVisible = false,
                UiButtonsEnabled = false,
                StopBtnEnabled = true,
                PauseBtnEnabled = true,
                Stopped = false,
                Init = false
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        /// <summary>
        /// props for stopped browsing, updates UpdateControlsEvent
        /// </summary>
        /// <param name="init">bool for initialization of the program</param>
        public void GetStoppedConfiguration(bool init)
        {
            _log.Called();

            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = false,
                PleaseWaitVisible = false,
                UiButtonsEnabled = true,
                StopBtnEnabled = false,
                PauseBtnEnabled = false,
                Stopped = true,
                Init = init
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        /// <summary>
        /// props for waiting status, updates UpdateControlsEvent
        /// </summary>
        public void GetWaitConfiguration()
        {
            _log.Called();

            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = false,
                PleaseWaitVisible = true,
                UiButtonsEnabled = false,
                StopBtnEnabled = false,
                PauseBtnEnabled = false,
                Stopped = false,
                Init = false
            };

            _eventAggregator.GetEvent<UpdateControlsEvent>().Publish(uiModel);
        }

        /// <summary>
        /// props for browser window show less, updates UpdateBrowserEvent
        /// </summary>
        public void ShowLessBrowser()
        {
            _log.Called();

            BrowserPropertiesModel browser = new BrowserPropertiesModel()
            {
                WindowState = WindowState.Normal,
                IsBrowserFocused = false,
                IsOnTop = false,
                ResizeMode = ResizeMode.CanResize
            };

            _eventAggregator.GetEvent<UpdateBrowserEvent>().Publish(browser);
        }

        /// <summary>
        /// props for browser window show more running status, updates UpdateBrowserEvent
        /// </summary>
        public void ShowMoreBrowserRunning()
        {
            _log.Called();

            BrowserPropertiesModel browser;

            if (Debugger.IsAttached) // only for DEBUG
            {
                browser = new BrowserPropertiesModel()
                {
                    WindowState = WindowState.Maximized,
                    IsBrowserFocused = true,
                    IsOnTop = false,
                    ResizeMode = ResizeMode.CanResize
                };
            }
            else
            {
                browser = new BrowserPropertiesModel()
                {
                    WindowState = WindowState.Maximized,
                    IsBrowserFocused = true,
                    IsOnTop = true,
                    ResizeMode = ResizeMode.NoResize
                };
            }

            _eventAggregator.GetEvent<UpdateBrowserEvent>().Publish(browser);
        }

        /// <summary>
        /// props for browser window show more paused status, updates UpdateBrowserEvent
        /// </summary>
        public void ShowMoreBrowserPaused()
        {
            _log.Called();

            BrowserPropertiesModel browser = new BrowserPropertiesModel()
            {
                WindowState = WindowState.Maximized,
                IsBrowserFocused = true,
                IsOnTop = false,
                ResizeMode = ResizeMode.CanResize
            };

            _eventAggregator.GetEvent<UpdateBrowserEvent>().Publish(browser);
        }
    }
}
