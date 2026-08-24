using MeterImportV2.Interfaces;
using MeterImportV2.Service;
using MeterImportV2.ViewModel;
using Microsoft.Extensions.DependencyInjection;
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
