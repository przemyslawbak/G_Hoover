using System.Threading.Tasks;

namespace G_Hoover.Services.Audio
{
    public interface IAudioService
    {
        Task RecordAudioSampleAsync();
        Task<string> ProcessAudioSampleAsync();
    }
}