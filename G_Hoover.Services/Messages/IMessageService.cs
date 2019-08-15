using G_Hoover.Models;
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
        Dictionary<string, string> GetDisplayMessage();
        List<string> MethodList { get; set; }
        MessageDictionariesModel LoadDictionaries();
    }
}
