using G_Hoover.Models;

namespace G_Hoover.Services.Controls
{
    public interface IControlsService
    {
        UiPropertiesModel GetPausedConfiguration();
        UiPropertiesModel GetStoppedConfiguration();
        UiPropertiesModel GetStartedConfiguration();
        UiPropertiesModel GetWaitConfiguration();
    }
}