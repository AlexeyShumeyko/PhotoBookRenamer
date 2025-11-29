using System.ComponentModel;
using System.Drawing;

namespace PhotoBookRenamer.Models
{
    public class ImageFile : INotifyPropertyChanged
    {
        private string _filePath = string.Empty;
        private string _fileName = string.Empty;
        private long _fileSize;
        private DateTime _creationTime;
        private Size _dimensions;
        private bool _isSelected;

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        public long FileSize
        {
            get => _fileSize;
            set
            {
                _fileSize = value;
                OnPropertyChanged(nameof(FileSize));
            }
        }

        public DateTime CreationTime
        {
            get => _creationTime;
            set
            {
                _creationTime = value;
                OnPropertyChanged(nameof(CreationTime));
            }
        }

        public Size Dimensions
        {
            get => _dimensions;
            set
            {
                _dimensions = value;
                OnPropertyChanged(nameof(Dimensions));
            }
        }

        public int PixelCount => Dimensions.Width * Dimensions.Height;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
