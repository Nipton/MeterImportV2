using MeterImportV2.Models;
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

            foreach (var reading in result.Readings)
            {
                Console.WriteLine(reading);
            }
            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(readings);
            Assert.Equal(3, count);

            Assert.Contains(meter1, readings);
            Assert.Contains(meter2, readings);
            Assert.Contains(meter3, readings);
        }
    }
}
