using G_Hoover.Desktop.ViewModels;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Desktop.Startup
{
    class PopulateDictionaries
    {
        IMessageService _messageService;
        FileService _fileService;
        BrowserViewModel _browserVM;

        public PopulateDictionaries(IMessageService messageService)
        {
            _messageService = messageService;

            PopulateList();
        }

        private void PopulateList()
        {
            _messageService.MethodList = LoadMethods();
        }

        private List<string> LoadMethods()
        {
            List<string> methods = new List<string>();
            methods.Add(nameof(_fileService.GetNewListFromFileAsync));
            methods.Add(nameof(_fileService.RemoveOldLogs));
            methods.Add(nameof(_browserVM.OnUploadCommandAsync));

            return methods;
        }
    }
}
