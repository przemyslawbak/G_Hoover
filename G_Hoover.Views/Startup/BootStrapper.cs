using Autofac;
using G_Hoover.ViewModels;
using G_Hoover.Views.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Views.Startup
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
