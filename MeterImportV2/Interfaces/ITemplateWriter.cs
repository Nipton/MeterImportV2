using MeterImportV2.Models;
using MeterImportV2.Models.Enums;

namespace MeterImportV2.Interfaces
{
    public interface ITemplateWriter
    {
        IEnumerable<ImportMessage> Write(Dictionary<(string Serial, string TariffZone), MeterReading> readings, string path, ResourceType resourceType, Company company);
    }
}
