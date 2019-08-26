using System.Runtime.CompilerServices;

namespace G_Hoover.Services.Logging
{
    public interface ILogService
    {
        void Called(params object[] values);
        void Ended(params object[] values);
        void Info(string value);
        void Error(string value);
        void Prop(object value, [CallerMemberName] string propertyName = null);
    }
}