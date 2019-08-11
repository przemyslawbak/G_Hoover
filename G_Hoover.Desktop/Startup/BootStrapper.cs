using Autofac;
using G_Hoover.Desktop.ViewModels;
using G_Hoover.Desktop.Views;

namespace G_Hoover.Desktop.Startup
{
    public class BootStrapper
    {
        /// <summary>
        /// IoC container
        /// </summary>
        /// <returns></returns>
        public static IContainer BootStrap()
        {
            var builder = new ContainerBuilder();



            builder.RegisterType<BrowserView>().AsSelf();
            builder.RegisterType<BrowserViewModel>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
