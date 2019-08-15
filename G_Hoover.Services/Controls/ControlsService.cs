using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using G_Hoover.Models;

namespace G_Hoover.Services.Controls
{
    public class ControlsService : IControlsService
    {
        public UiPropertiesModel GetPausedConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = true,
                PleaseWaitVisible = false,
                UiButtonsEnabled = true,
                StopBtnEnabled = true,
                PauseBtnEnabled = true,
            };

            return uiModel;
        }

        public UiPropertiesModel GetStartedConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = false,
                PleaseWaitVisible = false,
                UiButtonsEnabled = false,
                StopBtnEnabled = true,
                PauseBtnEnabled = true,
            };

            return uiModel;
        }

        public UiPropertiesModel GetStoppedConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = false,
                PleaseWaitVisible = false,
                UiButtonsEnabled = true,
                StopBtnEnabled = false,
                PauseBtnEnabled = false,
            };

            return uiModel;
        }

        public UiPropertiesModel GetWaitConfiguration()
        {
            UiPropertiesModel uiModel = new UiPropertiesModel()
            {
                Paused = true,
                PleaseWaitVisible = true,
                UiButtonsEnabled = false,
                StopBtnEnabled = false,
                PauseBtnEnabled = false,
            };

            return uiModel;
        }
    }
}
