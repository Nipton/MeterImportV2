namespace MeterImportV2.Models
{
    public class ReaderResult
    {
        public IEnumerable<MeterReading> Readings { get; }
        public IEnumerable<ImportMessage> ImportMessages { get; }
        public ReaderResult(IEnumerable<MeterReading> readings, IEnumerable<ImportMessage> messages)
        {
            Readings = readings;
            ImportMessages = messages;
        }
    }
}
