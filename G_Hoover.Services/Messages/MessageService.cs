using G_Hoover.Models;
using System.Collections.Generic;

namespace G_Hoover.Services.Messages
{
    public class MessageService : IMessageService
    {

        public MessageService()
        {
            MethodList = new List<string>();
        }
        public List<string> MethodList { get; set; }

        public Dictionary<string, string> GetMessagesInfo()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], MethodList[0] + " called. ");
            messages.Add(MethodList[1], MethodList[1] + " called. ");
            messages.Add(MethodList[2], MethodList[2] + " called. ");
            messages.Add(MethodList[3], MethodList[3] + " called. ");
            messages.Add(MethodList[4], MethodList[4] + " called. ");
            messages.Add(MethodList[5], MethodList[5] + " called. ");
            messages.Add(MethodList[6], MethodList[6] + " called. ");
            messages.Add(MethodList[7], MethodList[7] + " called. ");
            messages.Add(MethodList[8], MethodList[8] + " called. ");
            messages.Add(MethodList[9], MethodList[9] + " called. ");
            messages.Add(MethodList[10], MethodList[10] + " called. ");
            messages.Add(MethodList[11], MethodList[11] + " called. ");
            messages.Add(MethodList[12], MethodList[12] + " called. ");

            return messages;
        }

        public Dictionary<string, string> GetMessagesResult()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], MethodList[0] + " completed. ");
            messages.Add(MethodList[1], MethodList[1] + " completed. ");
            messages.Add(MethodList[2], MethodList[2] + " completed. ");
            messages.Add(MethodList[3], MethodList[3] + " completed. ");
            messages.Add(MethodList[4], MethodList[4] + " completed. ");
            messages.Add(MethodList[5], MethodList[5] + " completed. ");
            messages.Add(MethodList[6], MethodList[6] + " completed. ");
            messages.Add(MethodList[7], MethodList[7] + " completed. ");
            messages.Add(MethodList[8], MethodList[8] + " completed. ");
            messages.Add(MethodList[9], MethodList[9] + " completed. ");
            messages.Add(MethodList[10], MethodList[10] + " completed. ");
            messages.Add(MethodList[11], MethodList[11] + " completed. ");
            messages.Add(MethodList[12], MethodList[12] + " completed. ");

            return messages;
        }

        public Dictionary<string, string> GetMessagesError()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], MethodList[0] + " exception. ");
            messages.Add(MethodList[1], MethodList[1] + " exception. ");
            messages.Add(MethodList[2], MethodList[2] + " exception. ");
            messages.Add(MethodList[3], MethodList[3] + " exception. ");
            messages.Add(MethodList[4], MethodList[4] + " exception. ");
            messages.Add(MethodList[5], MethodList[5] + " exception. ");
            messages.Add(MethodList[6], MethodList[6] + " exception. ");
            messages.Add(MethodList[7], MethodList[7] + " exception. ");
            messages.Add(MethodList[8], MethodList[8] + " empty string. ");
            messages.Add(MethodList[9], MethodList[9] + " empty string. ");
            messages.Add(MethodList[10], MethodList[10] + " completed. ");
            messages.Add(MethodList[11], MethodList[11] + " completed. ");
            messages.Add(MethodList[12], MethodList[12] + " exception. ");

            return messages;
        }

        public Dictionary<string, string> GetDisplayMessage()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], "");
            messages.Add(MethodList[1], "");
            messages.Add(MethodList[2], "");
            messages.Add(MethodList[3], "");
            messages.Add(MethodList[4], "");
            messages.Add(MethodList[5], "");
            messages.Add(MethodList[6], "");
            messages.Add(MethodList[7], "");
            messages.Add(MethodList[8], "");
            messages.Add(MethodList[9], "");
            messages.Add(MethodList[10], "");
            messages.Add(MethodList[11], "");
            messages.Add(MethodList[12], "");

            return messages;
        }

        public MessageDictionariesModel LoadDictionaries()
        {
            MessageDictionariesModel messages = new MessageDictionariesModel()
            {
                MessagesDisplay = GetDisplayMessage(),
                MessagesError = GetMessagesError(),
                MessagesInfo = GetMessagesInfo(),
                MessagesResult = GetMessagesResult()
            };

            return messages;
        }
    }
}
