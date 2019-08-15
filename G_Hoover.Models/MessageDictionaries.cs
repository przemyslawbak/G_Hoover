using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Models
{
    public class MessageDictionaries
    {
        public Dictionary<string, string> MessagesInfo { get; set; }
        public Dictionary<string, string> MessagesError { get; set; }
        public Dictionary<string, string> MessagesResult { get; set; }
        public Dictionary<string, string> MessagesDisplay { get; set; }
    }
}
