namespace G_Hoover.Services.Config
{
    public interface IAppConfig
    {
        string AudioApiKey { get; }
        string AudioApiRegion { get; }

        string GetFilePath();
        int GetPhraseNo();
        string GetSearchPhrase();
    }
}