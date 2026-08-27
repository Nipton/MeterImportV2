using MeterImportV2.Interfaces;
using MeterImportV2.Models.Enums;
using MeterImportV2.Readers;
using MeterImportV2.Writers;
using Microsoft.Extensions.DependencyInjection;

namespace MeterImportV2.Service
{
    public class ImportServiceFactory : IImportServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public ImportServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IReadingsReader CreateReader(ResourceType resourceType, Company company)
        {
            return (resourceType, company) switch
            {
                (ResourceType.Electricity, Company.Dial) => _serviceProvider.GetRequiredService<DialElectricityReader>(),
                (ResourceType.ColdWater, Company.Dial) => _serviceProvider.GetRequiredService<DialColdWaterReader>(),
                (ResourceType.Electricity, Company.ComfortRule) => _serviceProvider.GetRequiredService<ComfortAndSmartElectricityReader>(),
                (ResourceType.Electricity, Company.Smart) => _serviceProvider.GetRequiredService<ComfortAndSmartElectricityReader>(),
                _ => throw new NotSupportedException($"Нет ридера для {resourceType} и {company}"),
            };
        }
        public ITemplateWriter CreateWriter()
        {
            return _serviceProvider.GetRequiredService<Writer>();
        }
    }
}
