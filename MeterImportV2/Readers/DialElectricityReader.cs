using MeterImportV2.Exceptions;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using Microsoft.Extensions.Options;

namespace MeterImportV2.Readers
{
    public class DialElectricityReader
    {
        private readonly ColumnSettings _column;
        public DialElectricityReader(IOptions<ReaderSettings> settings)
        {
            _column = settings.Value.Get(ResourceType.Electricity, Company.Dial);
           
        }
        public void Read()
        {
            ValidateColumnSettings(_column);

        }

        private void ValidateColumnSettings(ColumnSettings column)
        {
            if (string.IsNullOrWhiteSpace(column.AddressColumn) || string.IsNullOrWhiteSpace(column.SerialColumn) || string.IsNullOrWhiteSpace(column.TariffZoneColumn) || string.IsNullOrWhiteSpace(column.ConsumptionColumn))
                throw new ReaderException("Не удалось прочитать значение из файла настроек. Проверьте файл settings.json");
        }
    }
}
