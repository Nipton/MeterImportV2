using MeterImportV2.Models;

namespace MeterImportV2.Interfaces
{
    public interface IReadingsReader
    {
        ReaderResult Read(string path);
    }
}
