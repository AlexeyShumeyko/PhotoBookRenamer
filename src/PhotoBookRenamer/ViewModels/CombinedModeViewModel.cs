using PhotoBookRenamer.Models;
using PhotoBookRenamer.Services;

namespace PhotoBookRenamer.ViewModels
{
    public class CombinedModeViewModel : ViewModelBase, IExportable
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IFileService _fileService;

        public CombinedModeViewModel(MainViewModel mainViewModel, IFileService fileService)
        {
            _mainViewModel = mainViewModel;
            _fileService = fileService;
        }

        public Project Project => _mainViewModel.CurrentProject;

        public Task ExportAsync()
        {
            // Временная реализация
            return Task.CompletedTask;
        }
    }
}
