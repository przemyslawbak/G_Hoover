﻿using G_Hoover.Desktop.ViewModels;
using G_Hoover.Services.Browsing;
using G_Hoover.Services.Buttons;
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
        private readonly ButtonsService _buttonsService;
        private readonly BrowseService _browseService;
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
            methods.Add(nameof(_buttonsService.ExecuteUploadButtonAsync)); //2
            methods.Add(nameof(_buttonsService.ExecuteBuildButton)); //3
            methods.Add(nameof(_buttonsService.ExecuteStartButtonAsync)); //4
            methods.Add(nameof(_browseService.CollectDataAsync)); //5
            methods.Add(nameof(_buttonsService.ExecuteStopButton)); //6
            methods.Add(nameof(_buttonsService.ExecutePauseButton)); //7
            methods.Add(nameof(_browseService.LoopCollectingAsync)); //8
            methods.Add(nameof(_browseService.GetNewRecordAsync)); //9
            methods.Add(nameof(_browseService.CheckForCaptchaAsync)); //10
            methods.Add(nameof(_buttonsService.ExecuteConnectionButtonAsync)); //11

            return methods;
        }
    }
}
