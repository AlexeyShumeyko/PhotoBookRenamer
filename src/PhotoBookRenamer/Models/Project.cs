using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PhotoBookRenamer.Models
{
    public class Project : INotifyPropertyChanged
    {
        private ApplicationMode _currentMode;
        private string _projectName = "New Project";
        private string _outputPath = string.Empty;

        public ApplicationMode CurrentMode
        {
            get => _currentMode;
            set
            {
                _currentMode = value;
                OnPropertyChanged(nameof(CurrentMode));
            }
        }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }

        public string OutputPath
        {
            get => _outputPath;
            set
            {
                _outputPath = value;
                OnPropertyChanged(nameof(OutputPath));
            }
        }

        public ObservableCollection<Book> Books { get; } = new();
        public ObservableCollection<ImageFile> AvailableImages { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ApplicationMode
    {
        StartScreen,
        UniqueFolders,
        Combined
    }
}
