namespace MeterImportV2.Models
{
    public class ReaderResult
    {
        public Dictionary<(string Serial, string TariffZone), MeterReading> Readings { get; }
        public IEnumerable<ImportMessage> ImportMessages { get; }
        public ReaderResult(Dictionary<(string Serial, string TariffZone), MeterReading> readings, IEnumerable<ImportMessage> messages)
        {
            Readings = readings;
            ImportMessages = messages;
        }
    }
}
