using PhotoBookRenamer.Models;
using PhotoBookRenamer.Services;

namespace PhotoBookRenamer.ViewModels
{
    public class CombinedModeViewModel : ViewModelBase, IExportable
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IFileService _fileService;
        private bool _isLoading;

        public CombinedModeViewModel(MainViewModel mainViewModel, IFileService fileService)
        {
            _mainViewModel = mainViewModel;
            _fileService = fileService;
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public Project Project => _mainViewModel.CurrentProject;

        public System.Windows.Input.ICommand SwitchToStartCommand => _mainViewModel.SwitchToStartCommand;

        public Task ExportAsync()
        {
            // Временная реализация
            System.Windows.MessageBox.Show("Combined Mode export - в разработке");
            return Task.CompletedTask;
        }
    }
}
