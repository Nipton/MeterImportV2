using MeterImportV2.Interfaces;
using MeterImportV2.Models;
using MeterImportV2.Readers;
using MeterImportV2.Service;
using MeterImportV2.ViewModel;
using MeterImportV2.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MeterImportV2
{
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; } = null!;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var services = new ServiceCollection();

            try
            {
                var configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("settings.json", optional: false, reloadOnChange: true).Build();
                services.Configure<AppSettings>(configuration);
            }
            catch (Exception)
            {
                MessageBox.Show($"Ошибка чтения настроек. Проверьте файл settings.json", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IFileValidator, FileValidator>();
            services.AddSingleton<IImportServiceFactory, ImportServiceFactory>();
            services.AddSingleton<DialElectricityReader>();
            services.AddSingleton<DialColdWaterReader>();
            services.AddSingleton<Writer>();

            Services = services.BuildServiceProvider();
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}
