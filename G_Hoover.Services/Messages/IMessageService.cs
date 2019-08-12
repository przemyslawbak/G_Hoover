using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Services.Messages
{
    public interface IMessageService
    {
        Dictionary<string, string> GetMessagesInfo();
        Dictionary<string, string> GetMessagesError();
        Dictionary<string, string> GetMessagesResult();
        string GetCallerName([CallerMemberName] string caller = null);
        List<string> MethodList { get; set; }
    }
}
