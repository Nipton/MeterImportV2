using MeterImportV2.Interfaces;
using System.IO;

namespace MeterImportV2.Service
{
    public class FileValidator : IFileValidator
    {
        private readonly string[] validExtensions = { ".xlsx", ".xls", ".xlsm", ".xlsb" };
        public ValidationResult ValidateFilePath(string name, string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return ValidationResult.Fail($"{name} не выбран!");
            }
            var extension = Path.GetExtension(path).ToLowerInvariant();
            if (!validExtensions.Contains(extension))
            {
                return ValidationResult.Fail($"Неподдерживаемый формат файла. Разрешенные расширения: {string.Join(", ", validExtensions)}");
            }
            return ValidationResult.Success();
        }
    }
}
