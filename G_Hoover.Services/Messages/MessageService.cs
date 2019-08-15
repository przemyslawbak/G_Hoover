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
            messages.Add(MethodList[0], "Uploading data from the file. ");
            messages.Add(MethodList[1], "Deleting old log.txt file. ");
            messages.Add(MethodList[2], "Clicked upload button. ");
            messages.Add(MethodList[3], "Clicked phrase builder button. ");
            messages.Add(MethodList[4], "Loading phrase form the file. ");
            messages.Add(MethodList[5], "Saving new phrase to the file. ");
            messages.Add(MethodList[6], "Start collecting data. ");
            messages.Add(MethodList[7], "CollectDataAsync Task started. ");
            messages.Add(MethodList[8], "Clikced Stop button. ");
            messages.Add(MethodList[9], "Clikced Pause button. ");

            return messages;
        }

        public Dictionary<string, string> GetMessagesResult()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], "Uploaded List<string> from the file. ");
            messages.Add(MethodList[1], "Removed old log.txt file. ");
            messages.Add(MethodList[2], "Data uploaded successfully. ");
            messages.Add(MethodList[3], "New phrase: ");
            messages.Add(MethodList[4], "Loaded phrase. ");
            messages.Add(MethodList[5], "Phrase saved. ");
            messages.Add(MethodList[6], "Finished collecting data. ");
            messages.Add(MethodList[7], "GetRecordAsync completed. ");
            messages.Add(MethodList[8], "TokenSource cancelled. ");
            messages.Add(MethodList[9], "Paused property changed to ");

            return messages;
        }

        public Dictionary<string, string> GetMessagesError()
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();
            messages.Add(MethodList[0], "Error when reading data from the file. ");
            messages.Add(MethodList[1], "Could not delete old log.txt. ");
            messages.Add(MethodList[2], "Could not upload data from the file. ");
            messages.Add(MethodList[3], "Could not build the phrase. ");
            messages.Add(MethodList[4], "Could not load phrase from the file. ");
            messages.Add(MethodList[5], "Could not save phrase to the file. ");
            messages.Add(MethodList[6], "Error when collecting data. ");
            messages.Add(MethodList[7], "TokenSource cancelled. ");
            messages.Add(MethodList[8], "Problem with token cancellation. ");
            messages.Add(MethodList[9], "Failed to pause the program. ");

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
