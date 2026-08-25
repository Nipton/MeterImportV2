using ClosedXML.Excel;
using MeterImportV2.Exceptions;
using MeterImportV2.Models;
using Microsoft.Extensions.Options;

namespace MeterImportV2.Writer
{
    public class ElectricityWriter
    {
        private readonly TemplateColumnSettings _column;
        public ElectricityWriter(IOptions<AppSettings> settings)
        {
            _column = settings.Value.Writer;
        }
        public IEnumerable<ImportMessage> Write(IEnumerable<MeterReading> readings, string path)
        {
            ValidateColumnSettings(_column);
            using var workbook = new XLWorkbook(path);
            if (workbook.Worksheets.Count == 0)
                throw new ReaderException("Файл не содержит листов");
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(_column.HeaderRow);
            List<ImportMessage> messages = new();

            return messages;
        }

        private void ValidateColumnSettings(TemplateColumnSettings column)
        {
            if (string.IsNullOrWhiteSpace(column.AddressColumn) || string.IsNullOrWhiteSpace(column.SerialColumn) || string.IsNullOrWhiteSpace(column.TariffZoneColumn) || string.IsNullOrWhiteSpace(column.PreviousReadingColumn) || string.IsNullOrWhiteSpace(column.CurrentReadingColumn))
                throw new ReaderException("Не удалось прочитать значение из файла настроек. Проверьте файл settings.json");
            if (_column.HeaderRow <= 0)
                throw new ReaderException("Не указана строка заголовка в настройках");
        }
    }
}
