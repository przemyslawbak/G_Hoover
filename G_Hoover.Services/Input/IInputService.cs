using System.Threading.Tasks;

namespace G_Hoover.Services.Input
{
    public interface IInputService
    {
        Task ClickCheckboxInputAsync();
        Task ClickAudioChallangeInputAsync();
        Task ClickPlayInputAsync(bool inputCorrection);
        Task ClickNewAudioChallengeInputAsync(bool inputCorrection);
        Task ClickTextBoxInputAsync();
        Task EnterResulInputAsync(string audioResult);
        Task ClickSendResultInputAsync();
        Task ClickSearchBarInputAsync();
        Task EnterPhraseInputAsync(string phrase);
        Task ClickSearchButtonInputAsync();
    }
}