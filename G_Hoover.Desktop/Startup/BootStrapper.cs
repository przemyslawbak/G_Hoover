using Autofac;
using G_Hoover.Desktop.ViewModels;
using G_Hoover.Desktop.Views;
using G_Hoover.Services.Audio;
using G_Hoover.Services.Browsing;
using G_Hoover.Services.Buttons;
using G_Hoover.Services.Connection;
using G_Hoover.Services.Controls;
using G_Hoover.Services.Files;
using G_Hoover.Services.Input;
using G_Hoover.Services.Messages;
using G_Hoover.Services.Scrap;
using MvvmDialogs;
using Prism.Events;

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

            builder.RegisterType<EventAggregator>()
              .As<IEventAggregator>().SingleInstance();

            builder.RegisterType<ScrapService>()
              .As<IScrapService>().SingleInstance();

            builder.RegisterType<AudioService>()
              .As<IAudioService>().SingleInstance();

            builder.RegisterType<InputService>()
              .As<IInputService>().SingleInstance();

            builder.RegisterType<BrowseService>()
              .As<IBrowseService>().SingleInstance();

            builder.RegisterType<ConnectionService>()
              .As<IConnectionService>().SingleInstance();

            builder.RegisterType<ControlsService>()
              .As<IControlsService>().SingleInstance();

            builder.RegisterType<DialogService>()
              .As<IDialogService>().SingleInstance();

            builder.RegisterType<FileService>()
              .As<IFileService>().SingleInstance();

            builder.RegisterType<MessageService>()
              .As<IMessageService>().SingleInstance();

            builder.RegisterType<ButtonsService>()
              .As<IButtonsService>().SingleInstance();

            builder.RegisterType<BrowserView>().AsSelf();
            builder.RegisterType<BrowserViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<PhraseView>().AsSelf();
            builder.RegisterType<PhraseViewModel>().AsSelf().SingleInstance().WithParameter(new NamedParameter("searchPhrase", ""));
            builder.RegisterType<PopulateDictionaries>().AsSelf().SingleInstance();

            return builder.Build();
        }
    }
}
