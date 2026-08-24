using MeterImportV2.Service;

namespace MeterImportV2.Interfaces
{
    public interface IFileValidator
    {
        ValidationResult ValidateFilePath(string name, string? path);
    }
}
