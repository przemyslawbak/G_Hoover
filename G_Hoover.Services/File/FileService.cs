using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Services.Files
{
    public class FileService : IFileService
    {
        public async Task<List<string>> GetNewListFromFileAsync(string filePath)
        {
            List<string> list = new List<string>();

            if (File.Exists(filePath) && filePath.Length > 0)
            {
                using (var reader = File.OpenText(filePath))
                {
                    var fileText = await reader.ReadToEndAsync();
                    string[] array = fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    foreach(string line in array)
                    {
                        list.Add(line);
                    }
                }

                return list;
            }
            else
            {
                list.Clear();

                return list;
            }
        }
    }
}
