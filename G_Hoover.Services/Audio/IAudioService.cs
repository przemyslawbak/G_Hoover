using System.Threading.Tasks;

namespace G_Hoover.Services.Audio
{
    public interface IAudioService
    {
        Task RecordAudioSample(string key);
    }
}