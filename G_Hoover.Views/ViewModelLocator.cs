using Autofac;
using G_Hoover.Startup;
using G_Hoover.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover
{
    public class ViewModelLocator
    {
        IContainer container = BootStrapper.BootStrap();

        public BrowserViewModel BrowserViewModel
        {
            get
            {
                return container.Resolve<BrowserViewModel>();
            }
        }
    }
}
