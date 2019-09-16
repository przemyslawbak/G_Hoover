using System.Collections.Generic;
using System.Threading.Tasks;
using CSCore.Codecs.WAV;
using CSCore.SoundIn;
using G_Hoover.Models;
using Microsoft.CognitiveServices.Speech.Audio;

namespace G_Hoover.Services.Files
{
    public interface IFilingService
    {
        Task<List<string>> GetNewListFromFileAsync(string filePath);
        Task SaveNewResultAsync(ResultObjectModel result, string phrase);
        void DeleteOldAudio();
        bool CheckAudioFile();
        string GetAudioFilePath();
        string ProsessText(string audioResult);
        void DeleteResultsFile();
        Task<int> GetHowManyRecordsAsync();
    }
}