using G_Hoover.Models;

namespace G_Hoover.Services.Controls
{
    public interface IControlsService
    {
        void GetPausedConfiguration();
        void GetStoppedConfiguration();
        void GetStartedConfiguration();
        void GetWaitConfiguration();
        void ExecuteStopButton();
    }
}