using MeterImportV2.Models;
using MeterImportV2.Models.Enums;

namespace MeterImportV2.Interfaces
{
    public interface ITemplateWriter
    {
        IEnumerable<ImportMessage> Write(IEnumerable<MeterReading> readingsList, string path, ResourceType resourceType, Company company);
    }
}
