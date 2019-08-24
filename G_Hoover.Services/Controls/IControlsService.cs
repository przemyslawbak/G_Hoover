using System.Collections.Generic;
using System.Threading.Tasks;
using G_Hoover.Models;

namespace G_Hoover.Services.Controls
{
    public interface IControlsService
    {
        void GetPausedConfiguration();
        void GetStoppedConfiguration(bool init);
        void GetStartedConfiguration();
        void GetWaitConfiguration();
        void ShowLessBrowser();
        void ShowMoreBrowserRunning();
        void ShowMoreBrowserPaused();
    }
}