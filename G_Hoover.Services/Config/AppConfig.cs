using System.Configuration;

namespace G_Hoover.Services.Config
{
    public class AppConfig
    {
        public string AudioApiKey { get; } = ConfigurationManager.AppSettings["audioApiKey"];
        public string AudioApiRegion { get; } = ConfigurationManager.AppSettings["audioApiRegion"];
    }
}
