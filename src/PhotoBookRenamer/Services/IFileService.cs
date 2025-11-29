using PhotoBookRenamer.Models;
using System.Drawing;

namespace PhotoBookRenamer.Services
{
    public interface IFileService
    {
        Task<List<ImageFile>> LoadImagesFromFolderAsync(string folderPath);
        Task<List<ImageFile>> LoadImagesFromFilesAsync(IEnumerable<string> filePaths);
        Task<bool> ValidateFolderContentsAsync(IEnumerable<string> folderPaths);
        Task<string> CopyImageAsync(string sourcePath, string destinationPath);
        Task<Size> GetImageDimensionsAsync(string filePath);
    }
}
