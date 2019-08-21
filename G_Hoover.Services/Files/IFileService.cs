using System.Collections.Generic;
using System.Threading.Tasks;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using G_Hoover.Models;

namespace G_Hoover.Services.Files
{
    public interface IFileService
    {
        Task<List<string>> GetNewListFromFileAsync(string filePath);
        void RemoveOldLogs();
        Task<string> LoadPhraseAsync();
        Task SavePhraseAsync(string searchPhrase);
        Task SaveNewResult(ResultObjectModel result, string phrase);
        WaveWriter CreateNewWaveWriter(WasapiCapture capture);
        bool CheckAudioFile();
    }
}