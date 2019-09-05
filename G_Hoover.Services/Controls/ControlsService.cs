using System.Diagnostics;
using System.Windows;
using G_Hoover.Events;
using G_Hoover.Models;
using Params_Logger;
using Prism.Events;

namespace G_Hoover.Services.Controls
{
    public class ControlsService : IControlsService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IParamsLogger _log;

        public ControlsService(IEventAggregator eventAggregator, IParamsLogger log)
        {
            _log = log;
            _eventAggregator = eventAggregator;
        }

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
