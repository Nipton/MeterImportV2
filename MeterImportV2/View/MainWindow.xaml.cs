using MeterImportV2.ViewModel;
using System.Windows;
using System.Windows.Threading;

namespace MeterImportV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            var availableHeight = SystemParameters.WorkArea.Height;
            var headerHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight * 2;
            MaxHeight = availableHeight + headerHeight;
            vm.JournalVisibilityChanged += OnLogVisibilityChanged;

        }
        private void OnLogVisibilityChanged(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {

                var screen = SystemParameters.WorkArea;
                Left = (screen.Width - ActualWidth) / 2 + screen.Left;
                Top = (screen.Height - ActualHeight) / 2 + screen.Top;
            }), DispatcherPriority.ContextIdle);
        }
    }
}