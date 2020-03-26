
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;

namespace M.OBD2
{
    public class ProcessItem : INotifyPropertyChanged
    {
        public long id { get; set; }
        public string Name { get; set; }
        private string _value;

        public string Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged("Value");
            }
        }

        public string Units { get; set; }
        public string ImageSource { get; set; }
        public Color NameColor { get; set; }
        public Color ValueColor { get; set; }
        public Color UnitsColor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ProcessItems : ObservableCollection<ProcessItem>
    {
    }
}
