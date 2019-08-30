namespace G_Hoover.Services.Config
{
    public interface IAppConfig
    {
        string AudioApiKey { get; }
        string AudioApiRegion { get; }
        string LogFile { get; }
        string ResultFile { get; }
        string AudioFile { get; }

        string GetFilePath();
        int GetPhraseNo();
        string GetSearchPhrase();
        void SaveSearchPhrase(string searchPhrase);
        void SavePhraseNo(int phraseNo);
        void SaveFilePath(string filePath);
    }
}