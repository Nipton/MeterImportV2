namespace MeterImportV2.Models
{
    public class ReadingsColumnSettings
    {
        public string? AddressColumn { get; set; } = string.Empty;
        public string SerialColumn { get; set; } = string.Empty;
        public string ConsumptionColumn { get; set; } = string.Empty;
        public string? TariffZoneColumn { get; set; } = string.Empty;
        public int HeaderRow {  get; set; }
    }
}
