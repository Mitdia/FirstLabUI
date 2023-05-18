using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ViewModelFirstLabUI
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName)); 
    }   
}

