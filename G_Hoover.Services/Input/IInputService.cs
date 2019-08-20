using System.Threading.Tasks;

namespace G_Hoover.Services.Input
{
    public interface IInputService
    {
        Task ClickCheckboxInputAsync();
        Task AudioChallangeInputAsync();
    }
}