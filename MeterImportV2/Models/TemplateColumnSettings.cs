namespace MeterImportV2.Models
{
    public class TemplateColumnSettings
    {
        public string? AddressColumn { get; set; } = string.Empty;
        public string SerialColumn { get; set; } = string.Empty;
        public string? TariffZoneColumn { get; set; } = string.Empty;
        public string PreviousReadingColumn { get; set; } = string.Empty;
        public string CurrentReadingColumn { get; set; } = string.Empty;
        public int HeaderRow { get; set; }
    }
}
