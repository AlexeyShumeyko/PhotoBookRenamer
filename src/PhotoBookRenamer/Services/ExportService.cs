using PhotoBookRenamer.Models;
using System.IO;

namespace PhotoBookRenamer.Services
{
    public class ExportService : IExportService
    {
        private readonly IFileService _fileService;

        public ExportService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task ExportProjectAsync(Project project, string outputPath)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            var tasks = new List<Task>();

            for (int bookIndex = 0; bookIndex < project.Books.Count; bookIndex++)
            {
                var book = project.Books[bookIndex];
                var bookNumber = bookIndex + 1;

                // Экспорт обложки
                if (book.Cover != null)
                {
                    var coverFileName = GenerateFileName(bookNumber, 0);
                    var coverOutputPath = Path.Combine(outputPath, coverFileName);
                    tasks.Add(_fileService.CopyImageAsync(book.Cover.FilePath, coverOutputPath));
                }

                // Экспорт разворотов
                for (int spreadIndex = 0; spreadIndex < book.Spreads.Count; spreadIndex++)
                {
                    var spread = book.Spreads[spreadIndex];
                    var spreadNumber = spreadIndex + 1;
                    var spreadFileName = GenerateFileName(bookNumber, spreadNumber);
                    var spreadOutputPath = Path.Combine(outputPath, spreadFileName);
                    tasks.Add(_fileService.CopyImageAsync(spread.FilePath, spreadOutputPath));
                }
            }

            await Task.WhenAll(tasks);
        }

        public string GenerateFileName(int bookIndex, int fileIndex)
        {
            var bookPart = bookIndex.ToString("D2");
            var filePart = fileIndex.ToString("D2");
            return $"{bookPart}-{filePart}.jpg";
        }
    }
}
