using MeterImportV2.Models.Enums;

namespace MeterImportV2.Interfaces
{
    public interface IImportServiceFactory
    {
        IReadingsReader CreateReader(ResourceType resourceType, Company company);
        ITemplateWriter CreateWriter();
    }
}
