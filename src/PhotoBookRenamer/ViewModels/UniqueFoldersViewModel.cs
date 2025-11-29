using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using PhotoBookRenamer.Models;
using PhotoBookRenamer.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoBookRenamer.ViewModels
{
    public class UniqueFoldersViewModel : ViewModelBase, IExportable
    {
        private readonly MainViewModel _mainViewModel;
        private readonly IFileService _fileService;
        private bool _isLoading;
        private string _statusMessage = "Выберите папки для начала работы";

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

        public string StatusMessage
        {
            get => _statusMessage;
            set => Set(ref _statusMessage, value);
        }

        public System.Windows.Input.ICommand SelectFoldersCommand { get; private set; }
        public System.Windows.Input.ICommand ExportCommand { get; private set; }
        public System.Windows.Input.ICommand ChangeCoverCommand { get; private set; }
        public System.Windows.Input.ICommand SwitchToStartCommand { get; private set; }


        private void InitializeCommands()
        {
            SelectFoldersCommand = new RelayCommand(async () => await SelectFoldersAsync());

            ExportCommand = new RelayCommand(async () => await ExportAsync(),
                () => Books.Any() && Books.All(b => b.Cover != null));

            ChangeCoverCommand = new RelayCommand<ImageFile>(async (image) => await ChangeCoverAsync(image));

            SwitchToStartCommand = new RelayCommand(() =>
            _mainViewModel.CurrentViewModel = new StartScreenViewModel(_mainViewModel));
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
            StatusMessage = "Загрузка и проверка папок...";

            try
            {
                // Валидация папок
                if (!await ValidateFoldersAsync(folderPaths))
                    return;

                Books.Clear();
                Project.Books.Clear();

                // Загрузка изображений из каждой папки
                var loadTasks = folderPaths.Select(async folderPath =>
                {
                    var images = await _fileService.LoadImagesFromFolderAsync(folderPath);
                    if (images.Any())
                    {
                        var book = CreateBookFromImages(folderPath, images);
                        Project.Books.Add(book);

                        var bookViewModel = new BookViewModel(book, this);
                        await Application.Current.Dispatcher.InvokeAsync(() => Books.Add(bookViewModel));
                    }
                });

                await Task.WhenAll(loadTasks);

                StatusMessage = $"Загружено {Books.Count} книг. Проверьте обложки и нажмите 'Экспорт' когда будете готовы.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки папок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Ошибка при загрузке папок";
            }
            finally
            {
                IsLoading = false;
            }

            //try
            //{
            //    var isValid = await _fileService.ValidateFolderContentsAsync(folderPaths);
            //    if (!isValid)
            //    {
            //        System.Windows.MessageBox.Show(
            //            "Разное количество файлов найдено. Удалите некорректную папку.",
            //            "Ошибка валидации",
            //            System.Windows.MessageBoxButton.OK,
            //            System.Windows.MessageBoxImage.Error);

            //        return;
            //    }

            //    Books.Clear();
            //    Project.Books.Clear();

            //    foreach (var folderPath in folderPaths)
            //    {
            //        var images = await _fileService.LoadImagesFromFolderAsync(folderPath);
            //        if (images.Any())
            //        {
            //            var book = CreateBookFromImages(folderPath, images);
            //            Project.Books.Add(book);

            //            var bookViewModel = new BookViewModel(book);
            //            Books.Add(bookViewModel);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    System.Windows.MessageBox.Show($"Ошибка загрузки папок: {ex.Message}", "Ошибка",
            //        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            //}
            //finally
            //{
            //    IsLoading = false;
            //}
        }

        private async Task<bool> ValidateFoldersAsync(string[] folderPaths)
        {
            // Проверка существования папок
            foreach (var folderPath in folderPaths)
            {
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show($"Папка не существует: {folderPath}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            // Проверка что все папки содержат только JPG/JPEG
            foreach (var folderPath in folderPaths)
            {
                var files = Directory.GetFiles(folderPath);
                var nonImageFiles = files.Where(f =>
                    !f.ToLower().EndsWith(".jpg") && !f.ToLower().EndsWith(".jpeg"));

                if (nonImageFiles.Any())
                {
                    MessageBox.Show($"Папка содержит не-JPG файлы: {folderPath}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }

            // Проверка одинакового количества файлов
            var fileCounts = new List<int>();
            foreach (var folderPath in folderPaths)
            {
                var jpgFiles = Directory.GetFiles(folderPath)
                    .Count(f => f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".jpeg"));
                fileCounts.Add(jpgFiles);
            }

            if (fileCounts.Distinct().Count() > 1)
            {
                MessageBox.Show("Разное количество файлов найдено в папках. Удалите некорректную папку.",
                    "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
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

        private async Task ChangeCoverAsync(ImageFile? newCover)
        {
            if (newCover == null) return;

            // Находим книгу, к которой принадлежит это изображение
            var bookVm = Books.FirstOrDefault(b =>
                b.Spreads.Contains(newCover) || b.Cover == newCover);

            if (bookVm != null && bookVm.Cover != newCover)
            {
                // Меняем обложку
                var oldCover = bookVm.Cover;

                bookVm.Cover = newCover;
                bookVm.Spreads.Remove(newCover);

                if (oldCover != null)
                    bookVm.Spreads.Insert(0, oldCover);

                // Обновляем оригинальную книгу в проекте
                var book = Project.Books.First(b => b.Name == bookVm.Name);
                book.Cover = newCover;
                book.Spreads.Clear();
                foreach (var spread in bookVm.Spreads)
                    book.Spreads.Add(spread);
            }
        }

        public async Task ExportAsync()
        {
            if (!Books.Any())
            {
                MessageBox.Show("Нет книг для экспорта", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "JPEG Files|*.jpg",
                Title = "Выберите папку для экспорта",
                FileName = "PhotoBook_Export",
                OverwritePrompt = false
            };

            if (dialog.ShowDialog() == true)
            {
                var outputFolder = Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(outputFolder))
                {
                    try
                    {
                        IsLoading = true;
                        StatusMessage = "Экспорт файлов...";

                        var exportService = new ExportService(_fileService);
                        await exportService.ExportProjectAsync(Project, outputFolder);

                        MessageBox.Show($"Экспорт успешно завершен!\nФайлы сохранены в: {outputFolder}",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        StatusMessage = $"Экспорт завершен. Файлы в: {outputFolder}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);

                        StatusMessage = "Ошибка при экспорте";
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }
    }

    public class BookViewModel : ViewModelBase
    {
        private readonly Book _book;
        private readonly UniqueFoldersViewModel _parent;

        public BookViewModel(Book book, UniqueFoldersViewModel parent)
        {
            _book = book;
            _parent = parent;
        }

        public string Name => _book.Name;
        public ImageFile? Cover
        {
            get => _book.Cover;
            set
            {
                _book.Cover = value;
                RaisePropertyChanged(nameof(Cover));
            }
        }
        public ObservableCollection<ImageFile> Spreads => _book.Spreads;

        public System.Windows.Input.ICommand ChangeCoverCommand => _parent.ChangeCoverCommand;
    }
}
