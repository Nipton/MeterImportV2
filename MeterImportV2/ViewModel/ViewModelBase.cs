using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MeterImportV2.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null) 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if(Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
