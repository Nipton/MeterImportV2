namespace MeterImportV2.Models
{
    public class MeterReading
    {
        public string Serial { get; set; } = string.Empty;
        public decimal Consumption { get; set; }
        public string? Address { get; set; } = string.Empty;
        public string? TariffZone { get; set; } = string.Empty;
        public MeterReading(string serial, decimal consumption, string address, string tariffZone)
        {
            Serial = serial;
            Consumption = consumption;
            Address = address;
            TariffZone = tariffZone;
        }
    }
}
