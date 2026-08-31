using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using MeterImportV2.Readers;
using Microsoft.Extensions.Options;
using System.IO;

namespace MeterImportV2.IntegrationTests
{
    public class DialElectricityReaderTests
    {
        private readonly string _testFilePath;
        private readonly DialElectricityReader _reader;
        public DialElectricityReaderTests()
        {
            // Arrange
            _testFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "DialElectricityReaderTest.xlsx");
            var settings = Options.Create(new AppSettings
            {
                Readers = new Dictionary<string, Dictionary<string, ReadingsColumnSettings>>
                {
                    ["Electricity"] = new Dictionary<string, ReadingsColumnSettings>
                    {
                        ["Dial"] = new ReadingsColumnSettings { AddressColumn = "A", SerialColumn = "B", TariffZoneColumn = "C", ConsumptionColumn = "D",  HeaderRow = 1 }
                    }
                }
            });
            _reader = new DialElectricityReader(settings);
        }
        [Fact]
        public void Read_ReturnsCorrectResult()
        {
            // Arrange
            var meter1 = new MeterReading("1", 100, "Дом 1", "ДЕНЬ");
            var meter2 = new MeterReading("1", 50, "Дом 1", "НОЧЬ");
            var meter3 = new MeterReading("2", 200, "Дом 2", "КРУГЛОСУТОЧНЫЙ");
            var meter4 = new MeterReading("10", 2, "Дом 10", "КРУГЛОСУТОЧНЫЙ");
            // Act
            var result = _reader.Read(_testFilePath);
            var readings = result.Readings.Values;
            var count = readings.Count;          

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(readings);
            Assert.Equal(4, count);
            Assert.Contains(meter1, readings);
            Assert.Contains(meter2, readings);
            Assert.Contains(meter3, readings);
            Assert.Contains(meter4, readings);
        }
        [Fact]
        public void Read_ConflictingSerials_ReturnsErrors()
        {
            // Arrange
            var expectedSerials = new[] { "3", "4", "5", "6", "7", "12", "16" };

            // Act
            var result = _reader.Read(_testFilePath);
            var errorMessages = result.ImportMessages.Where(x => x.MessageType == MessageType.Error).ToList();
            var count = errorMessages.Count();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(errorMessages);
            Assert.Equal(7, count);
            foreach (var serial in expectedSerials)
            {
                Assert.Contains(errorMessages, m => m.Message.Contains($"Невозможно определить тарифную зону для ПУ {serial}, так как обнаружены конфликтующие записи"));
            }    
        }
        [Fact]
        public void Read_InvalidData_ReturnsWarning()
        {
            // Act
            var result = _reader.Read(_testFilePath);
            var warningMessages = result.ImportMessages.Where(x => x.MessageType == MessageType.Warning).ToList();
            var count = warningMessages.Count();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(warningMessages);
            Assert.Equal(6, count);

            Assert.Contains(warningMessages, m => m.Message.Contains("Пропущен адрес для ПУ 8, строка 22"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Не удалось прочитать показания для ПУ 11, строка 28"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Пропущен серийный номер в строке 24"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Не удалось прочитать показания для ПУ 15, строка 37"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Некорректное показание -100 для ПУ 13, строка 33"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Некорректное показание 1000000 для ПУ 14, строка 35"));
        }
    }
}
