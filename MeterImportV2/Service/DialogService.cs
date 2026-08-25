using MeterImportV2.Interfaces;
using Microsoft.Win32;
using System.Windows;

namespace MeterImportV2.Service
{
    public class DialogService : IDialogService
    {
        public string? SelectFile(string title)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = title,
                Filter = "Excel файлы|*.xlsx;*.xls;*.xlsm;*.xlsb"
            };
            bool? result = dialog.ShowDialog();
            return result == true ? dialog.FileName : null;
        }
        public void ShowWarning(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        public void ShowError(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
