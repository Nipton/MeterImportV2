using ClosedXML.Excel;
using MeterImportV2.Exceptions;
using MeterImportV2.Helpers;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using Microsoft.Extensions.Options;

namespace MeterImportV2.Readers
{
    public class ComfortAndSmartElectricityReader : BaseReader
    {
        public ComfortAndSmartElectricityReader(IOptions<AppSettings> settings) 
            : base(settings.Value.GetReaderSettings(ResourceType.Electricity, Company.ComfortRule)){ }
        protected override void ProcessRow(IXLRow row)
        {
            var serialCell = row.Cell(_column.SerialColumn);
            var mergetRange = row.Worksheet.MergedRanges.FirstOrDefault(r => r.Contains(serialCell));
            bool isTopCell = mergetRange?.FirstCell().Address.ToString() == serialCell.Address.ToString();

            var serialNumber = serialCell.GetString();
            var serialAbove = GetSerialAbove(row);
            var serialBelow = GetSerialBelow(row);
            if ((mergetRange != null && isTopCell) || (!string.IsNullOrWhiteSpace(serialNumber) && serialNumber.Equals(serialBelow)))
            {
                if (!ValidateSerialNumber(row, serialNumber))
                    return;
                if (!TryGetConsumption(row, serialNumber, out var consumption))
                    return;
                if (!TryGetConsumption(row.RowBelow(), serialNumber, out var consumptionBelow))
                    return;
                TryAddReading(new MeterReading(serialNumber, consumption, TariffZone.DayTariff));
                TryAddReading(new MeterReading(serialNumber, consumptionBelow, TariffZone.NightTariff));
            }
            else if ((mergetRange != null && !isTopCell)|| serialNumber.Equals(serialAbove))
                return;
            else  
            {
                if (!ValidateSerialNumber(row, serialNumber))
                    return;
                if (!TryGetConsumption(row, serialNumber, out var consumption))
                    return;
                TryAddReading(new MeterReading(serialNumber, consumption, TariffZone.ConstantTariff));
            }
        }
        private bool ValidateSerialNumber(IXLRow row, string serialNumber)
        {
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                _messages.Add(CreateMessage($"Пропущен серийный номер в строке {row.RowNumber()}", MessageType.Warning));
                return false;
            }
            return true;
        }
        private bool TryGetConsumption(IXLRow row, string serial, out decimal consumption)
        {
            string consumptionString = row.Cell(_column.ConsumptionColumn).GetString();
            if (!decimal.TryParse(consumptionString, out consumption))
            {
                _messages.Add(CreateMessage($"Не удалось прочитать показания для ПУ {serial}, строка {row.RowNumber()}", MessageType.Warning));
                return false;
            }
            if (consumption < 0 || consumption > 999999)
            {
                _messages.Add(CreateMessage($"Некорректное показание {consumption} для ПУ {serial}, строка {row.RowNumber()}", MessageType.Warning));
                return false;
            }
            return true;
        }
        private string GetSerialBelow(IXLRow row)
        {
            var lastRow = row.Worksheet.LastRowUsed()?.RowNumber();
            if (lastRow == null || row.RowNumber() >= lastRow)
                return string.Empty;
            return row.RowBelow().Cell(_column.SerialColumn).GetString();
        }
        private string GetSerialAbove(IXLRow row)
        {
            if (row.RowNumber() <= 1)
                return string.Empty;
            return row.RowAbove().Cell(_column.SerialColumn).GetString();
        }
        protected override void ValidateColumnSettings()
        {
            if (string.IsNullOrWhiteSpace(_column.SerialColumn) || string.IsNullOrWhiteSpace(_column.ConsumptionColumn))
                throw new ImportException("Не удалось прочитать значение из файла настроек. Проверьте файл settings.json");
            if (_column.HeaderRow <= 0)
                throw new ImportException("Не указана строка заголовка в настройках");
        }
    }
}
