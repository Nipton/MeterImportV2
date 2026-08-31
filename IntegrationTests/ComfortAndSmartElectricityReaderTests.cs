using MeterImportV2.Exceptions;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using MeterImportV2.Readers;
using Microsoft.Extensions.Options;
using System.IO;

namespace MeterImportV2.IntegrationTests
{
    public class ComfortAndSmartElectricityReaderTests
    {
        private readonly string _testFilePath;
        private readonly ComfortAndSmartElectricityReader _reader;
        public ComfortAndSmartElectricityReaderTests()
        {
            // Arrange
            _testFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "ComfortRuleReadingsTest.xlsx");
            var settings = Options.Create(new AppSettings 
            { 
                Readers = new Dictionary<string, Dictionary<string, ReadingsColumnSettings>>
                { 
                    ["Electricity"] = new Dictionary<string, ReadingsColumnSettings> 
                    {
                        ["ComfortRule"] = new ReadingsColumnSettings { SerialColumn = "A", ConsumptionColumn = "B", HeaderRow = 1 }
                    }
                }
            });
            _reader = new ComfortAndSmartElectricityReader(settings);
        }
        [Fact]
        public void Read_ReturnsCorrectResult()
        {
            // Arrange
            var meter1 = new MeterReading("1", 1, "КРУГЛОСУТОЧНЫЙ");
            var meter2 = new MeterReading("11", 20, "ДЕНЬ");
            var meter3 = new MeterReading("11", 10, "НОЧЬ");
            // Act
            var result = _reader.Read(_testFilePath);
            var readings = result.Readings.Values;
            int count = result.Readings.Count();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(readings);
            Assert.Equal(3, count);

            Assert.Contains(meter1, readings);
            Assert.Contains(meter2, readings);
            Assert.Contains(meter3, readings);
        }
        [Fact]
        public void Read_ConflictingSerials_ReturnsErrors()
        {
            // Arrange
            var expectedSerials = new[] { "100", "250", "300", "333", "444", "700", "800" };
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
            Assert.Equal(5, count);

            Assert.Contains(warningMessages, m => m.Message.Contains("Не удалось прочитать показания для ПУ 200, строка 7"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Пропущен серийный номер в строке 12"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Не удалось прочитать показания для ПУ 400, строка 13"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Некорректное показание -555 для ПУ 500, строка 14"));
            Assert.Contains(warningMessages, m => m.Message.Contains("Некорректное показание 1000001 для ПУ 600, строка 15"));
        }
        [Fact]
        public void Read_EmptyWorkbook_ThrowsException()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xlsx");
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook();
                workbook.AddWorksheet("Sheet1");
                workbook.SaveAs(tempPath);

                var exception = Assert.Throws<ImportException>(() => _reader.Read(tempPath));
                Assert.Contains("Не найден лист с данными", exception.Message);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
