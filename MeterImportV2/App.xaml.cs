using MeterImportV2.Interfaces;
using MeterImportV2.Models;
using MeterImportV2.Service;
using MeterImportV2.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Windows;

namespace MeterImportV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
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
                services.Configure<ReaderSettings>(configuration.GetSection("Readers"));
            }
            catch (Exception)
            {
                MessageBox.Show($"Ошибка чтения настроек", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IFileValidator, FileValidator>();

            Services = services.BuildServiceProvider();
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }

}
