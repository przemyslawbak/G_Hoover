using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace G_Hoover.Desktop.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged //INotify
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
