using System.Windows;
using System.Windows.Input;

namespace G_Hoover.Models
{
    public class BrowserPropertiesModel
    {
        public WindowState WindowState { get; set; }
        public bool IsBrowserFocused { get; set; }
        public bool IsOnTop { get; set; }
        public ResizeMode ResizeMode { get; set; }
    }
}
