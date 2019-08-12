using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Services.Messages
{
    public class MessageService : IMessageService
    {
        public MessageService()
        {
            MethodList = new List<string>();
        }
        public List<string> MethodList { get; set; }

        /// <summary>
        /// returns name of the caller
        /// </summary>
        /// <param name="caller">caller name</param>
        /// <returns>caller name</returns>
        public string GetCallerName([CallerMemberName] string caller = null)
        {
            return caller;
        }

        public Dictionary<string, string> GetMessagesError()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], "Could not upload data. ");

            return messages;
        }

        public Dictionary<string, string> GetMessagesInfo()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], "Uploading data from the file.");

            return messages;
        }

        public Dictionary<string, string> GetMessagesResult()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], "Data Uploaded successfully.");

            return messages;
        }
    }
}
