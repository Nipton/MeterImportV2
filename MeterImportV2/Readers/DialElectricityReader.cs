using ClosedXML.Excel;
using MeterImportV2.Exceptions;
using MeterImportV2.Interfaces;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using Microsoft.Extensions.Options;

namespace MeterImportV2.Readers
{
    public class DialElectricityReader : IReadingsReader
    {
        private readonly ReadingsColumnSettings _column;
        public DialElectricityReader(IOptions<AppSettings> settings)
        {
            _column = settings.Value.GetReaderSettings(ResourceType.Electricity, Company.Dial);
        }
        public ReaderResult Read(string path)
        {
            ValidateColumnSettings(_column);
            using var workbook = new XLWorkbook(path);
            if (workbook.Worksheets.Count == 0)
                throw new ReaderException("Файл не содержит листов");
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(_column.HeaderRow);
            List<MeterReading> readings = new();
            List<ImportMessage> messages = new();
            foreach (var row in rows)
            {
                int rowNumber = row.RowNumber();
                try
                {
                    string serial = row.Cell(_column.SerialColumn).GetString();
                    string address = row.Cell(_column.AddressColumn).GetString();
                    string tariffZone = row.Cell(_column.TariffZoneColumn).GetString();
                    tariffZone = string.IsNullOrWhiteSpace(tariffZone) ? "КРУГЛОСУТОЧНЫЙ" : tariffZone;
                    string consumptionString = row.Cell(_column.ConsumptionColumn).GetString();

                    if (string.IsNullOrWhiteSpace(serial))
                    {
                        messages.Add(new ImportMessage($"Пропущен серийный номер в строке {rowNumber}", MessageType.Warning));
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(address))
                    {
                        messages.Add(new ImportMessage($"Пропущен адрес для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                        continue;
                    }
                    if (!decimal.TryParse(consumptionString, out decimal consumption))
                    {
                        messages.Add(new ImportMessage($"Не удалось прочитать показания для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                        continue;
                    }
                    if (consumption < 0 || consumption > 999999)
                    {
                        messages.Add(new ImportMessage($"Некорректное показание {consumption} для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                        continue;
                    }

                    var meter = new MeterReading(serial, consumption, address, tariffZone);
                    readings.Add(meter);
                }
                catch (Exception)
                {
                    messages.Add(new ImportMessage($"Ошибка обработки строки {rowNumber}", MessageType.Error));
                }
            }
            return new ReaderResult(readings, messages);
        }
        private void ValidateColumnSettings(ReadingsColumnSettings column)
        {
            if (string.IsNullOrWhiteSpace(column.AddressColumn) || string.IsNullOrWhiteSpace(column.SerialColumn) || string.IsNullOrWhiteSpace(column.TariffZoneColumn) || string.IsNullOrWhiteSpace(column.ConsumptionColumn))
                throw new ReaderException("Не удалось прочитать значение из файла настроек. Проверьте файл settings.json");
            if (_column.HeaderRow <= 0)
                throw new ReaderException("Не указана строка заголовка в настройках");
        }
    }
}