using MeterImportV2.Exceptions;
using MeterImportV2.Models.Enums;

namespace MeterImportV2.Models
{
    public class AppSettings
    {
        public Dictionary<string, Dictionary<string, ReadingsColumnSettings>> Readers { get; set; } = new();
        public TemplateColumnSettings Writer { get; set; } = new();
        public ReadingsColumnSettings GetReaderSettings(ResourceType resource, Company company)
        {
            try
            {
                return Readers[resource.ToString()][company.ToString()];
            }
            catch (KeyNotFoundException)
            {
                throw new ImportException($"Не удалось найти настройки для ресурса '{resource}' и компании '{company}'. Проверьте файл настроек.");
            }
        }
    }
}
