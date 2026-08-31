using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using MeterImportV2.Readers;
using Microsoft.Extensions.Options;
using System.IO;

namespace MeterImportV2.IntegrationTests
{
    public class DialColdWaterReaderTests
    {
        private readonly string _testFilePath;
        private readonly DialColdWaterReader _reader;
        public DialColdWaterReaderTests()
        {
            // Arrange
            _testFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "DialColdWaterReaderTest.xlsx");
            var settings = Options.Create(new AppSettings
            {
                Readers = new Dictionary<string, Dictionary<string, ReadingsColumnSettings>>
                {
                    ["ColdWater"] = new Dictionary<string, ReadingsColumnSettings>
                    {
                        ["Dial"] = new ReadingsColumnSettings { SerialColumn = "A", ConsumptionColumn = "B", AddressColumn = "C", HeaderRow = 1 }
                    }
                }
            });
            _reader = new DialColdWaterReader(settings);
        }
        [Fact]
        public void Read_ReturnsCorrectResult()
        {
            // Arrange
            var meter = new MeterReading("1", 1, "КРУГЛОСУТОЧНЫЙ");
            // Act
            var result = _reader.Read(_testFilePath);
            var readings = result.Readings.Values;
            int count = result.Readings.Count();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(readings);
            Assert.Equal(1, count);
            Assert.Contains(meter, readings);
        }
        [Fact]
        public void Read_ConflictingSerials_ReturnsErrors()
        {
            // Act
            var result = _reader.Read(_testFilePath);
            var errorMessages = result.ImportMessages.Where(x => x.MessageType == MessageType.Error).ToList();
            var count = errorMessages.Count();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(errorMessages);
            Assert.Equal(1, count);
            Assert.Contains(errorMessages, m => m.Message.Contains("Невозможно определить тарифную зону для ПУ 2, так как обнаружены конфликтующие записи"));
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
            Assert.Equal(5, count);

            Assert.Contains(warningMessages, m => m.Message.Contains("Не удалось прочитать показания для ПУ 3, строка 5"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Не удалось прочитать показания для ПУ 4, строка 6"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Некорректное показание -50 для ПУ 5, строка 7"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Некорректное показание 1000000 для ПУ 6, строка 8"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Пропущен серийный номер в строке 9"));
        }
    }
}
