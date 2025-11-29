using PhotoBookRenamer.Models;

namespace PhotoBookRenamer.Services
{
    public interface IExportService
    {
        Task ExportProjectAsync(Project project, string outputPath);
        string GenerateFileName(int bookIndex, int fileIndex);
    }
}
