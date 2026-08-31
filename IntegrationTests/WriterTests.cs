using ClosedXML.Excel;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using MeterImportV2.Writers;
using Microsoft.Extensions.Options;
using System.IO;

namespace MeterImportV2.IntegrationTests
{
    public class WriterTests : IDisposable
    {
        private readonly string _testFilePath;
        private readonly string _resultPath;
        private readonly string _specialMetersPath;
        private readonly string _specialMetersTargetPath;
        private readonly Writer _writer;
        private readonly Dictionary<(string, string), MeterReading> _readings;
        public WriterTests()
        {
            _testFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "WriterTest.xlsx");
            _specialMetersPath = Path.Combine(AppContext.BaseDirectory, "TestData", "specialMeters.json");
            _resultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), Path.GetFileNameWithoutExtension(_testFilePath) + "_result.xlsx");
            _specialMetersTargetPath = Path.Combine(AppContext.BaseDirectory, "specialMeters.json");
            if (File.Exists(_specialMetersPath))
            {
                File.Copy(_specialMetersPath, _specialMetersTargetPath, true);
            }

            var settings = Options.Create(new AppSettings
            {
                Writer = new TemplateColumnSettings
                {
                    AddressColumn = "A",
                    SerialColumn = "B",
                    TariffZoneColumn = "C",
                    PreviousReadingColumn = "D",
                    CurrentReadingColumn = "E",
                    HeaderRow = 1
                }
            });

            _writer = new Writer(settings);

            _readings = new Dictionary<(string, string), MeterReading>
            {
                [("1", "КРУГЛОСУТОЧНЫЙ")] = new MeterReading("1", 50, "Дом 1", "КРУГЛОСУТОЧНЫЙ"),
                [("2", "ДЕНЬ")] = new MeterReading("2", 30, "Дом 2", "ДЕНЬ"),
                [("2", "НОЧЬ")] = new MeterReading("2", 20, "Дом 2", "НОЧЬ"),
                [("3", "КРУГЛОСУТОЧНЫЙ")] = new MeterReading("3", 100, "Дом 3", "КРУГЛОСУТОЧНЫЙ"),
                [("4", "КРУГЛОСУТОЧНЫЙ")] = new MeterReading("4", 50, "Дом 4", "КРУГЛОСУТОЧНЫЙ"),
                [("5", "ДЕНЬ")] = new MeterReading("5", 30, "Дом 5", "ДЕНЬ"),
                [("6", "НОЧЬ")] = new MeterReading("6", 20, "Дом 6", "НОЧЬ")
            };
        }
        [Fact]
        public void Write_ValidData_SavesSuccessfully()
        {

            // Act
            var result = _writer.Write(_readings, _testFilePath, ResourceType.Electricity, Company.Dial);
            using var workbook = new XLWorkbook(_resultPath);
            var sheet = workbook.Worksheet(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.DoesNotContain(result, m => m.MessageType == MessageType.Error);
            Assert.True(File.Exists(_resultPath));

            Assert.Equal(150, sheet.Cell("E2").GetValue<decimal>());
            Assert.Equal(80, sheet.Cell("E3").GetValue<decimal>());
            Assert.Equal(50, sheet.Cell("E4").GetValue<decimal>());
            Assert.Equal(60, sheet.Cell("E6").GetValue<decimal>());
            Assert.Contains(result, m => m.Message.Contains("Успешно обработано 6 записей из 9"));
        }
        [Fact]
        public void Write_WithSpecialMeters_CalculatesCorrectly()
        {
            // Act
            var result = _writer.Write(_readings, _testFilePath, ResourceType.Electricity, Company.Dial);
            using var workbook = new XLWorkbook(_resultPath);
            var sheet = workbook.Worksheet(1);

            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.DoesNotContain(result, m => m.MessageType == MessageType.Error);
            Assert.Equal(240, sheet.Cell("E5").GetValue<decimal>());
            Assert.Equal(160, sheet.Cell("E9").GetValue<decimal>());
        }
        [Fact]
        public void Write_InvalidData_ReturnsWarnings()
        {
            // Act
            var result = _writer.Write(_readings, _testFilePath, ResourceType.Electricity, Company.Dial);
            var warnings = result.Where(x => x.MessageType == MessageType.Warning).ToList();

            //Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(3, warnings.Count);
            Assert.Contains(warnings, m => m.Message.Contains("строка 7")); 
            Assert.Contains(warnings, m => m.Message.Contains("строка 8"));
            Assert.Contains(warnings, m => m.Message.Contains("Не найдены показания для ПУ 8, строка 10"));
        }

        public void Dispose()
        {
            if (File.Exists(_resultPath))
                File.Delete(_resultPath);
            if (File.Exists(_specialMetersTargetPath))
                File.Delete(_specialMetersTargetPath);
        }
    }
}
