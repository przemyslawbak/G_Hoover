using Autofac;
using G_Hoover.Desktop.Startup;

namespace G_Hoover.Desktop.ViewModels
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
