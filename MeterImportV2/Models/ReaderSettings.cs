using MeterImportV2.Models.Enums;

namespace MeterImportV2.Models
{
    public class ReaderSettings
    {
        public Dictionary<string, Dictionary<string, ColumnSettings>> Readers { get; set; } = new();
        public ColumnSettings Get(ResourceType resource, Company company)
        {
            return Readers[resource.ToString()][company.ToString()];
        }
    }
}
