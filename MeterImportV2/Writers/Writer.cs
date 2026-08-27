using ClosedXML.Excel;
using MeterImportV2.Exceptions;
using MeterImportV2.Helpers;
using MeterImportV2.Interfaces;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text.Json;

namespace MeterImportV2.Writers
{
    public class Writer : ITemplateWriter
    {
        private readonly TemplateColumnSettings _column;
        private const string MessagePrefix = "Шаблон: ";
        private readonly string specialMetersFileName = "specialMeters.json";
        public Writer(IOptions<AppSettings> settings)
        {
            _column = settings.Value.Writer;
        }
        public IEnumerable<ImportMessage> Write(IEnumerable<MeterReading> readingsList, string path, ResourceType resourceType, Company company)
        {
            ValidateColumnSettings();
            using var workbook = new XLWorkbook(path);
            if (workbook.Worksheets.Count == 0)
                throw new ImportException("Файл не содержит листов");
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RowsUsed().Skip(_column.HeaderRow);
            List<ImportMessage> messages = new();
            var readings = readingsList.ToDictionary(r => (r.Serial, r.TariffZone), r => r, new TupleStringComparer());
            Dictionary<string, List<SpecialMeter>>? specialMeters = null;
            if (resourceType == ResourceType.Electricity && company == Company.Dial)
                specialMeters = LoadSpecialMeters(messages);
            int successCount = 0;
            int rowCount = 0;
            foreach (var row in rows)
            {
                rowCount++;
                try
                {
                    if (ProcessRow(row, messages, readings, specialMeters))
                        successCount++;
                }
                catch (Exception)
                {
                    messages.Add(CreateMessage($"Ошибка обработки строки {row.RowNumber()}", MessageType.Error));
                }
            }
            messages.Add(new ImportMessage($"Успешно обработано {successCount} {WordForms.GetRecordsWord(successCount)} из {rowCount}", MessageType.Info));
            workbook.SaveAs(CreateResultPath(path));
            messages.Add(new ImportMessage("Файл был сохранен на рабочий стол", MessageType.Info));
            return messages;
        }
        private bool ProcessRow(IXLRow row, List<ImportMessage> messages, Dictionary<(string Serial, string TariffZone), MeterReading> readings, Dictionary<string, List<SpecialMeter>>? specialMeters)
        {
            int rowNumber = row.RowNumber();
            string serial = row.Cell(_column.SerialColumn).GetString();
            if (string.IsNullOrWhiteSpace(serial))
            {
                messages.Add(CreateMessage($"Пропущен серийный номер в строке {rowNumber}", MessageType.Warning));
                return false;
            }
            string tariffZone = TariffZoneHelper.Normalize(row.Cell(_column.TariffZoneColumn).GetString());
            if (!readings.TryGetValue((serial, tariffZone), out var meter))
            {
                messages.Add(CreateMessage($"Не найдены показания для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                return false;
            }
            string previousReadingString = row.Cell(_column.PreviousReadingColumn).GetString();
            if (!decimal.TryParse(previousReadingString, out decimal previousReading))
            {
                messages.Add(CreateMessage($"Не удалось прочитать показания для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                return false;
            }
            if (previousReading < 0 || previousReading > 999999)
            {
                messages.Add(CreateMessage($"Некорректное показание {previousReading} для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                return false;
            }
            var currentReading = CalculateCurrentReading(row, specialMeters, meter, serial, previousReading);
            if (currentReading == null)
            {
                messages.Add(CreateMessage($"Не удалось получить данные для распределенного ПУ {serial}", MessageType.Warning));
                return false;
            }
            row.Cell(_column.CurrentReadingColumn).Value = currentReading.Value;
            return true;
        }
        private decimal? CalculateCurrentReading(IXLRow row, Dictionary<string, List<SpecialMeter>>? specialMeters, MeterReading meter, string serial, decimal previousReading)
        {
            decimal currentReading = 0;
            if (specialMeters?.TryGetValue(serial, out var specialMeterList) == true)
            {
                string address = row.Cell(_column.AddressColumn).GetString();
                var specialMeter = specialMeterList.FirstOrDefault(x => string.Equals(x.Address, address, StringComparison.OrdinalIgnoreCase));
                if (specialMeter == null)
                    return null;
                currentReading = specialMeter.Calculate(meter.Consumption) + previousReading;
            }
            else
                currentReading = meter.Consumption + previousReading;
            return currentReading;
        }
        private Dictionary<string, List<SpecialMeter>> LoadSpecialMeters(List<ImportMessage> messages)
        {
            var path = Path.Combine(AppContext.BaseDirectory, specialMetersFileName);
            Dictionary<string, List<SpecialMeter>> specialMetersDict = new();
            try
            {
                var json = File.ReadAllText(path);
                var specialMeters = JsonSerializer.Deserialize<List<SpecialMeter>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SpecialMeter>();
                specialMetersDict = specialMeters.GroupBy(x => x.SerialNumber).ToDictionary(g => g.Key, g => g.ToList());
            }
            catch
            {
                messages.Add(new ImportMessage($"Ошибка чтения specialMeters.json. Словарь с особыми счетчиками не был загружен!", MessageType.Warning));
            }
            return specialMetersDict;
        }
        private void ValidateColumnSettings()
        {
            if (string.IsNullOrWhiteSpace(_column.AddressColumn) || string.IsNullOrWhiteSpace(_column.SerialColumn) || string.IsNullOrWhiteSpace(_column.TariffZoneColumn) || string.IsNullOrWhiteSpace(_column.PreviousReadingColumn) || string.IsNullOrWhiteSpace(_column.CurrentReadingColumn))
                throw new ImportException("Не удалось прочитать значение из файла настроек. Проверьте файл settings.json");
            if (_column.HeaderRow <= 0)
                throw new ImportException("Не указана строка заголовка в настройках");
        }
        private string CreateResultPath(string path)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var fileName = Path.GetFileNameWithoutExtension(path);
            return Path.Combine(desktopPath, fileName + "_result.xlsx");
        }
        private ImportMessage CreateMessage(string text, MessageType type)
        {
            return new ImportMessage($"{MessagePrefix}{text}", type);
        }
    }
}
