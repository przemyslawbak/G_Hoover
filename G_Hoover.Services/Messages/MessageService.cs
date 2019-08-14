﻿using System;
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
            messages.Add(MethodList[6], "List of phrases is finished. ");

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

            return messages;
        }
    }
}
