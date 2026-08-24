namespace MeterImportV2.Interfaces
{
    public interface IDialogService
    {
        string? SelectFile(string title);
        void ShowWarning(string message);
    }
}
