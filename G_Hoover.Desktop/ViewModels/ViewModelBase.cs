using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace G_Hoover.Desktop.ViewModels
{
    /// <summary>
    /// INotify implementation
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
