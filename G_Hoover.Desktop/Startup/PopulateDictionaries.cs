﻿using G_Hoover.Desktop.ViewModels;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using System.Collections.Generic;

namespace G_Hoover.Desktop.Startup
{
    class PopulateDictionaries
    {
        private readonly IMessageService _messageService;
        private readonly FileService _fileService;
        private readonly ControlsService _controlsService;
        private readonly BrowserViewModel _browserVM;

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
            methods.Add(nameof(_fileService.GetNewListFromFileAsync)); //0
            methods.Add(nameof(_fileService.RemoveOldLogs)); //1
            methods.Add(nameof(_browserVM.OnUploadCommandAsync)); //2
            methods.Add(nameof(_browserVM.OnBuildCommandAsync)); //3
            methods.Add(nameof(_fileService.LoadPhraseAsync)); //4
            methods.Add(nameof(_fileService.SavePhraseAsync)); //5
            methods.Add(nameof(_browserVM.OnStartCommandAsync)); //6
            methods.Add(nameof(_browserVM.CollectDataAsync)); //7
            methods.Add(nameof(_controlsService.ExecuteStopButton)); //8

            return methods;
        }
    }
}
