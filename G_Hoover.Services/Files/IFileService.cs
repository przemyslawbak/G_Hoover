using System.Collections.Generic;
using System.Threading.Tasks;
using G_Hoover.Models;

namespace G_Hoover.Services.Files
{
    public interface IFileService
    {
        Task<List<string>> GetNewListFromFileAsync(string filePath);
        void RemoveOldLogs();
        Task<string> LoadPhraseAsync();
        Task SavePhraseAsync(string searchPhrase);
        Task SaveNewResult(string stringResult);
    }
}