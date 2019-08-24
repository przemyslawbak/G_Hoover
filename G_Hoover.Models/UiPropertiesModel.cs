using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G_Hoover.Models
{
    public class UiPropertiesModel
    {
        public bool PleaseWaitVisible { get; set; }
        public bool Paused { get; set; }
        public bool UiButtonsEnabled { get; set; }
        public bool StopBtnEnabled { get; set; }
        public bool PauseBtnEnabled { get; set; }
        public bool Stopped { get; set; }
        public bool Init { get; set; }
    }
}
