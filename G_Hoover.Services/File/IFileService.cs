using System.Collections.Generic;
using System.Threading.Tasks;

namespace G_Hoover.Services.Files
{
    public interface IFileService
    {
        Task<List<string>> GetNewListFromFileAsync(string filePath);
    }
}