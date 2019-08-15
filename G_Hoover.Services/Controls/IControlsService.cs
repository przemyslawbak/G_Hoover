using System.Collections.Generic;
using System.Threading.Tasks;
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
        void ExecutePauseButton(bool paused);
        void ExecuteStartButton();
        Task<List<string>> ExecuteUploadButtonAsync(string filePath);
    }
}