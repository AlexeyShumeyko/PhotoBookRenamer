using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PhotoBookRenamer.Models;
using PhotoBookRenamer.Services;
using System.Collections.ObjectModel;
using System.IO;

namespace PhotoBookRenamer.ViewModels
{
    public class UniqueFoldersViewModel : ViewModelBase, IExportable
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IFileService _fileService;
        private bool _isLoading;

        public UniqueFoldersViewModel(MainViewModel mainViewModel, IFileService fileService)
        {
            _mainViewModel = mainViewModel;
            _fileService = fileService;

            InitializeCommands();
        }

        public ObservableCollection<BookViewModel> Books { get; } = new();
        public Project Project => _mainViewModel.CurrentProject;

        public bool IsLoading
        {
            get => _isLoading;
            set => Set(ref _isLoading, value);
        }

        public System.Windows.Input.ICommand SelectFoldersCommand { get; private set; }
        public System.Windows.Input.ICommand ExportCommand { get; private set; }

        private void InitializeCommands()
        {
            SelectFoldersCommand = new RelayCommand(async () => await SelectFoldersAsync());
            ExportCommand = new RelayCommand(async () => await ExportAsync());
        }

        private async Task SelectFoldersAsync()
        {
            var dialog = new OpenFolderDialog
            {
                Multiselect = true,
                Title = "Выберите папки с фотографиями"
            };

            if (dialog.ShowDialog() == true)
            {
                await LoadFoldersAsync(dialog.FolderNames);
            }
        }

        private async Task LoadFoldersAsync(string[] folderPaths)
        {
            IsLoading = true;

            try
            {
                var isValid = await _fileService.ValidateFolderContentsAsync(folderPaths);
                if (!isValid)
                {
                    System.Windows.MessageBox.Show(
                        "Разное количество файлов найдено. Удалите некорректную папку.",
                        "Ошибка валидации",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);

                    return;
                }

                Books.Clear();
                Project.Books.Clear();

                foreach (var folderPath in folderPaths)
                {
                    var images = await _fileService.LoadImagesFromFolderAsync(folderPath);
                    if (images.Any())
                    {
                        var book = CreateBookFromImages(folderPath, images);
                        Project.Books.Add(book);

                        var bookViewModel = new BookViewModel(book);
                        Books.Add(bookViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки папок: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Book CreateBookFromImages(string folderPath, List<ImageFile> images)
        {
            var book = new Book
            {
                Name = Path.GetFileName(folderPath)
            };

            var cover = images.OrderByDescending(img => img.PixelCount).First();
            book.Cover = cover;

            // Остальные изображения становятся разворотами
            foreach (var image in images.Where(img => img != cover))
            {
                book.Spreads.Add(image);
            }

            return book;
        }

        public async Task ExportAsync()
        {
            if (!Books.Any())
            {
                System.Windows.MessageBox.Show("Нет книг для экспорта", "Информация",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "JPEG Files|*.jpg",
                Title = "Выберите папку для экспорта",
                FileName = "export"
            };

            if (dialog.ShowDialog() == true)
            {
                var outputFolder = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(outputFolder))
                {
                    try
                    {
                        var exportService = new ExportService(_fileService);
                        await exportService.ExportProjectAsync(Project, outputFolder);

                        System.Windows.MessageBox.Show("Экспорт успешно завершен!", "Успех",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                }
            }
        }
    }

    public class BookViewModel : ViewModelBase
    {
        private readonly Book _book;

        public BookViewModel(Book book)
        {
            _book = book;
        }

        public string Name => _book.Name;
        public ImageFile? Cover => _book.Cover;
        public ObservableCollection<ImageFile> Spreads => _book.Spreads;
    }
}
