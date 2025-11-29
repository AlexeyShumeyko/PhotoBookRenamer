using PhotoBookRenamer.Models;
using System.Drawing;
using System.IO;

namespace PhotoBookRenamer.Services
{
    public class FileService : IFileService
    {
        private static readonly string[] SupportedExtensions = { ".jpg", ".jpeg", ".JPG", ".JPEG" };

        public async Task<List<ImageFile>> LoadImagesFromFolderAsync(string folderPath)
        {
            return await Task.Run(() =>
            {
                var imageFiles = new List<ImageFile>();

                try
                {
                    var files = Directory.GetFiles(folderPath)
                        .Where(f => SupportedExtensions.Contains(Path.GetExtension(f)))
                        .OrderBy(f => f)
                        .ToList();

                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        var imageFile = new ImageFile
                        {
                            FilePath = file,
                            FileName = Path.GetFileName(file),
                            FileSize = fileInfo.Length,
                            CreationTime = fileInfo.CreationTime
                        };

                        // Получаем размеры изображения
                        try
                        {
                            using var image = Image.FromFile(file);
                            imageFile.Dimensions = new Size(image.Width, image.Height);
                        }
                        catch
                        {
                            imageFile.Dimensions = new Size(0, 0);
                        }

                        imageFiles.Add(imageFile);
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Ошибка загрузки изображений из папки: {ex.Message}", ex);
                }

                return imageFiles;
            });
        }

        public async Task<List<ImageFile>> LoadImagesFromFilesAsync(IEnumerable<string> filePaths)
        {
            var tasks = filePaths
                .Where(f => SupportedExtensions.Contains(Path.GetExtension(f)))
                .Select(async filePath =>
                {
                    var fileInfo = new FileInfo(filePath);
                    var imageFile = new ImageFile
                    {
                        FilePath = filePath,
                        FileName = Path.GetFileName(filePath),
                        FileSize = fileInfo.Length,
                        CreationTime = fileInfo.CreationTime
                    };

                    try
                    {
                        imageFile.Dimensions = await GetImageDimensionsAsync(filePath);
                    }
                    catch
                    {
                        imageFile.Dimensions = new Size(0, 0);
                    }

                    return imageFile;
                });

            return (await Task.WhenAll(tasks)).ToList();
        }

        public async Task<bool> ValidateFolderContentsAsync(IEnumerable<string> folderPaths)
        {
            return await Task.Run(() =>
            {
                var folders = folderPaths.ToList();
                if (!folders.Any()) return false;

                var fileCounts = new List<int>();

                foreach (var folder in folders)
                {
                    if (!Directory.Exists(folder)) return false;

                    var jpgFiles = Directory.GetFiles(folder)
                        .Count(f => SupportedExtensions.Contains(Path.GetExtension(f)));

                    fileCounts.Add(jpgFiles);
                }

                return fileCounts.All(count => count == fileCounts[0]);
            });
        }

        public async Task<string> CopyImageAsync(string sourcePath, string destinationPath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var directory = Path.GetDirectoryName(destinationPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    File.Copy(sourcePath, destinationPath, true);
                    return destinationPath;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Ошибка копирования файла: {ex.Message}", ex);
                }
            });
        }

        public async Task<Size> GetImageDimensionsAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var image = Image.FromFile(filePath);
                    return new Size(image.Width, image.Height);
                }
                catch
                {
                    return new Size(0, 0);
                }
            });
        }
    }
}
