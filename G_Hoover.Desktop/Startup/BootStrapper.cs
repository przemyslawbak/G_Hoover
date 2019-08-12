using Autofac;
using G_Hoover.Desktop.ViewModels;
using G_Hoover.Desktop.Views;
using G_Hoover.Services.Files;
using G_Hoover.Services.Messages;
using MvvmDialogs;

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

            builder.RegisterType<DialogService>()
              .As<IDialogService>().SingleInstance();

            builder.RegisterType<FileService>()
              .As<IFileService>().SingleInstance();

            builder.RegisterType<MessageService>()
              .As<IMessageService>().SingleInstance();

            builder.RegisterType<BrowserView>().AsSelf();
            builder.RegisterType<BrowserViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<PopulateDictionaries>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
