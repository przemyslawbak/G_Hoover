namespace G_Hoover.Services.Logging
{
    public interface ILogService
    {
        void Called(params object[] values);
        void Ended(params object[] values);
        void Info(params object[] values);
        void Error(params object[] values);
    }
}