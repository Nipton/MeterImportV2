using ClosedXML.Excel;
using MeterImportV2.Exceptions;
using MeterImportV2.Helpers;
using MeterImportV2.Interfaces;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;

namespace MeterImportV2.Readers
{
    public abstract class BaseReader : IReadingsReader
    {
        protected readonly ReadingsColumnSettings _column;
        protected const string MessagePrefix = "Показания: ";
        protected readonly List<ImportMessage> _messages = new();
        private readonly HashSet<string> _invalidSerials = new();
        private readonly Dictionary<(string Serial, string TariffZone), MeterReading> _readings = new(new TupleStringComparer());
        protected BaseReader(ReadingsColumnSettings column)
        {
            _column = column;
        }
        public ReaderResult Read(string path)
        {
            _messages.Clear();
            _readings.Clear();
            _invalidSerials.Clear();
            ValidateColumnSettings();
            using var workbook = new XLWorkbook(path);
            if (workbook.Worksheets.Count == 0)
                throw new ImportException("Файл не содержит листов");
            var worksheet = workbook.Worksheets.LastOrDefault(x => x.LastRowUsed() != null);
            if (worksheet == null)
                throw new ImportException("Не найден лист с данными");
            var rows = worksheet.RowsUsed().Skip(_column.HeaderRow);
            _messages.Add(CreateMessage($"Используется лист '{worksheet.Name}'", MessageType.Info));
            foreach (var row in rows)
            {
                try
                {
                    ProcessRow(row);
                }
                catch (Exception)
                {
                    _messages.Add(CreateMessage($"Ошибка обработки строки {row.RowNumber()}", MessageType.Error));
                }
            }
            RemoveInvalidReadings();
            return new ReaderResult(_readings, _messages);
        }
        protected abstract void ProcessRow(IXLRow row);
        protected abstract void ValidateColumnSettings();
        protected virtual ImportMessage CreateMessage(string text, MessageType type)
        {
            return new ImportMessage($"{MessagePrefix}{text}", type);
        }
        protected void TryAddReading(MeterReading meter)
        {
            var key = (meter.Serial, meter.TariffZone);

            if (_readings.ContainsKey(key))
            {
                _invalidSerials.Add(meter.Serial);
                return;
            }
            if (meter.TariffZone == TariffZone.ConstantTariff)
            {
                if (_readings.ContainsKey((meter.Serial, TariffZone.DayTariff)) ||
                    _readings.ContainsKey((meter.Serial, TariffZone.NightTariff)))
                {
                    _invalidSerials.Add(meter.Serial);
                    return;
                }
            }
            else
            {
                if (_readings.ContainsKey((meter.Serial, TariffZone.ConstantTariff)))
                {
                    _invalidSerials.Add(meter.Serial);
                    return;
                }
            }

            _readings.Add(key, meter);
        }
        private void RemoveInvalidReadings()
        {
            if (_invalidSerials.Count == 0)
                return;
            foreach (var serial in _invalidSerials)
            {
                foreach (var key in _readings.Keys.Where(x => x.Serial == serial).ToList())
                {
                    _readings.Remove(key); 
                }
                _messages.Add(CreateMessage($"Невозможно определить тарифную зону для ПУ {serial}, так как обнаружены конфликтующие записи", MessageType.Error));
            }
        }
    }
}
