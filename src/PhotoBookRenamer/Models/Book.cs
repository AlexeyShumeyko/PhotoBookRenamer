using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PhotoBookRenamer.Models
{
    public class Book : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private ImageFile? _cover;
        private bool _isLocked;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public ObservableCollection<ImageFile> Pages { get; } = new();
        public ObservableCollection<ImageFile> Spreads { get; } = new();

        public ImageFile? Cover
        {
            get => _cover;
            set
            {
                _cover = value;
                OnPropertyChanged(nameof(Cover));
            }
        }

        public bool IsLocked
        {
            get => _isLocked;
            set
            {
                _isLocked = value;
                OnPropertyChanged(nameof(IsLocked));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}