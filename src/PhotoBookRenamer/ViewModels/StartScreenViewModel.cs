using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace PhotoBookRenamer.ViewModels
{
    public class StartScreenViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        private bool _isLoading;

        public StartScreenViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            SwitchToUniqueFoldersCommand = new RelayCommand(
            () => _mainViewModel.CurrentViewModel = new UniqueFoldersViewModel(_mainViewModel, _mainViewModel.FileService));

            SwitchToCombinedModeCommand = new RelayCommand(
                () => _mainViewModel.CurrentViewModel = new CombinedModeViewModel(_mainViewModel, _mainViewModel.FileService));
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public ICommand SwitchToUniqueFoldersCommand { get; }
        public ICommand SwitchToCombinedModeCommand { get; }
        public ICommand OpenFilesCommand => new RelayCommand(() => { });
        public ICommand OpenFoldersCommand => new RelayCommand(() => { });
        public ICommand ExportWithDialogCommand => new RelayCommand(() => { });
        public ICommand ResetCommand => new RelayCommand(() => { });
    }
}
