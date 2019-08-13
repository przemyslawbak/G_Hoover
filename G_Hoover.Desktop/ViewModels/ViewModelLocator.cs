﻿using Autofac;
using G_Hoover.Desktop.Startup;

namespace G_Hoover.Desktop.ViewModels
{
    public class ViewModelLocator
    {
        IContainer _container;
        public ViewModelLocator()
        {
            _container = BootStrapper.BootStrap();

            _container.Resolve<PopulateDictionaries>();
        }

        public BrowserViewModel BrowserViewModel
        {
            get
            {
                return _container.Resolve<BrowserViewModel>();
            }
        }
    }
}