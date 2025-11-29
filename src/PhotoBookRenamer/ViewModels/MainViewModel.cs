using PhotoBookRenamer.Models;
using PhotoBookRenamer.Services;
using GalaSoft.MvvmLight.Command;

namespace PhotoBookRenamer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly IExportService _exportService;
        private readonly IUndoRedoService _undoRedoService;
        private ViewModelBase _currentViewModel;

        public MainViewModel()
        {
            _fileService = new FileService();
            _exportService = new ExportService(_fileService);
            _undoRedoService = new UndoRedoService();

            CurrentViewModel = new StartScreenViewModel(this);

            InitializeCommands();
        }

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => Set(ref _currentViewModel, value);
        }

        public Project CurrentProject { get; } = new();

        public IFileService FileService => _fileService;
        public IExportService ExportService => _exportService;
        public IUndoRedoService UndoRedoService => _undoRedoService;

        // Команды
        public System.Windows.Input.ICommand SwitchToUniqueFoldersCommand { get; private set; }
        public System.Windows.Input.ICommand SwitchToCombinedModeCommand { get; private set; }
        public System.Windows.Input.ICommand UndoCommand { get; private set; }
        public System.Windows.Input.ICommand RedoCommand { get; private set; }
        public System.Windows.Input.ICommand ExportCommand { get; private set; }
        public System.Windows.Input.ICommand OpenFilesCommand { get; private set; }
        public System.Windows.Input.ICommand OpenFoldersCommand { get; private set; }
        public System.Windows.Input.ICommand ExportWithDialogCommand { get; private set; }
        public System.Windows.Input.ICommand ResetCommand { get; private set; }

        private void InitializeCommands()
        {
            SwitchToUniqueFoldersCommand = new RelayCommand(
                () => CurrentViewModel = new UniqueFoldersViewModel(this, _fileService));

            SwitchToCombinedModeCommand = new RelayCommand(
                () => CurrentViewModel = new CombinedModeViewModel(this, _fileService));

            UndoCommand = new RelayCommand(
                () => _undoRedoService.Undo(),
                () => _undoRedoService.CanUndo);

            RedoCommand = new RelayCommand(
                () => _undoRedoService.Redo(),
                () => _undoRedoService.CanRedo);

            ExportCommand = new RelayCommand(async () => await ExportAsync());

            OpenFilesCommand = new RelayCommand(() => { });
            OpenFoldersCommand = new RelayCommand(() => { });
            ExportWithDialogCommand = new RelayCommand(async () => await ExportAsync());
            ResetCommand = new RelayCommand(() => { });
        }

        private async Task ExportAsync()
        {
            try
            {
                if (CurrentViewModel is IExportable exportable)
                {
                    await exportable.ExportAsync();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
