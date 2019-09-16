using Autofac;
using G_Hoover.Desktop.Startup;

namespace G_Hoover.Desktop.ViewModels
{
    /// <summary>
    /// props with ViewModels called by windows when being opened
    /// </summary>
    public class ViewModelLocator
    {
        private readonly IContainer _container;

        public ViewModelLocator()
        {
            _container = BootStrapper.BootStrap();
        }

        public BrowserViewModel BrowserViewModel
        {
            get
            {
                return _container.Resolve<BrowserViewModel>();
            }
        }
        public PhraseViewModel PhraseViewModel
        {
            get
            {
                return _container.Resolve<PhraseViewModel>();
            }
        }
        public DeleteViewModel DeleteViewModel
        {
            get
            {
                return _container.Resolve<DeleteViewModel>();
            }
        }
    }
}
