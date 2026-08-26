namespace MeterImportV2.Models
{
    public class SpecialMeter
    {
        public string SerialNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Percentage { get; set; }

        public decimal Calculate(decimal consumption)
        {
            return consumption * Percentage;
        }
    }
}
