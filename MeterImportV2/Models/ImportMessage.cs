using MeterImportV2.Models.Enums;

namespace MeterImportV2.Models
{
    public class ImportMessage
    {
        public string Message { get; } = string.Empty;
        public MessageType MessageType { get; }
        public ImportMessage(string message, MessageType type)
        {
            MessageType = type;
            Message = message;
        }
    }
}
