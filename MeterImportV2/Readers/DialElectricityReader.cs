using ClosedXML.Excel;
using MeterImportV2.Exceptions;
using MeterImportV2.Helpers;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using Microsoft.Extensions.Options;

namespace MeterImportV2.Readers
{
    public class DialElectricityReader : BaseReader
    {
        public DialElectricityReader(IOptions<AppSettings> settings) : base(settings.Value.GetReaderSettings(ResourceType.Electricity, Company.Dial)){}
        protected override void ProcessRow(IXLRow row)
        {
            int rowNumber = row.RowNumber();
            string serial = row.Cell(_column.SerialColumn).GetString();
            if (string.IsNullOrWhiteSpace(serial))
            {
                _messages.Add(CreateMessage($"Пропущен серийный номер в строке {rowNumber}", MessageType.Warning));
                return;
            }
            string address = row.Cell(_column.AddressColumn).GetString();
            if (string.IsNullOrWhiteSpace(address))
            {
                _messages.Add(CreateMessage($"Пропущен адрес для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                return;
            }
            string tariffZone = TariffZone.Normalize(row.Cell(_column.TariffZoneColumn).GetString());

            string consumptionString = row.Cell(_column.ConsumptionColumn).GetString();
            if (!decimal.TryParse(consumptionString, out decimal consumption))
            {
                _messages.Add(CreateMessage($"Не удалось прочитать показания для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                return;
            }
            if (consumption < 0 || consumption > 999999)
            {
                _messages.Add(CreateMessage($"Некорректное показание {consumption} для ПУ {serial}, строка {rowNumber}", MessageType.Warning));
                return;
            }

            var meter = new MeterReading(serial, consumption, address, tariffZone);
            TryAddReading(meter);
        }
        protected override void ValidateColumnSettings()
        {
            if (string.IsNullOrWhiteSpace(_column.AddressColumn) || string.IsNullOrWhiteSpace(_column.SerialColumn) || string.IsNullOrWhiteSpace(_column.TariffZoneColumn) || string.IsNullOrWhiteSpace(_column.ConsumptionColumn))
                throw new ImportException("Не удалось прочитать значение из файла настроек. Проверьте файл settings.json");
            if (_column.HeaderRow <= 0)
                throw new ImportException("Не указана строка заголовка в настройках");
        }
    }
}