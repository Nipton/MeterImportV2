using ClosedXML.Excel;
using MeterImportV2.Exceptions;
using MeterImportV2.Interfaces;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;

namespace MeterImportV2.Readers
{
    public abstract class BaseReader : IReadingsReader
    {
        protected readonly ReadingsColumnSettings _column;
        protected const string MessagePrefix = "Показания: ";
        protected BaseReader(ReadingsColumnSettings column)
        {
            _column = column;
        }
        public ReaderResult Read(string path)
        {
            ValidateColumnSettings();
            using var workbook = new XLWorkbook(path);
            if (workbook.Worksheets.Count == 0)
                throw new ImportException("Файл не содержит листов");
            var worksheet = workbook.Worksheets.LastOrDefault(x => x.LastRowUsed() != null);
            if (worksheet == null)
                throw new ImportException("Не найден лист с данными");
            var rows = worksheet.RowsUsed().Skip(_column.HeaderRow);
            List<MeterReading> readings = new();
            List<ImportMessage> messages = new();
            messages.Add(CreateMessage($"Используется лист '{worksheet.Name}'", MessageType.Info));
            foreach (var row in rows)
            {
                try
                {
                    ProcessRow(row, messages, readings);
                }
                catch (Exception)
                {
                    messages.Add(CreateMessage($"Ошибка обработки строки {row.RowNumber()}", MessageType.Error));
                }
            }
            return new ReaderResult(readings, messages);
        }
        protected abstract void ProcessRow(IXLRow row, List<ImportMessage> messages, List<MeterReading> readings);
        protected abstract void ValidateColumnSettings();
        protected virtual ImportMessage CreateMessage(string text, MessageType type)
        {
            return new ImportMessage($"{MessagePrefix}{text}", type);
        }
    }
}
