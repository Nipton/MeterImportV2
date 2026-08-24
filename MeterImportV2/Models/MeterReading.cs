namespace MeterImportV2.Models
{
    public class MeterReading
    {
        public string Address { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public decimal Consumption { get; set; } 
        public string? TariffZoneColumn { get; set; } = string.Empty;
    }
}
