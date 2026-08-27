using System.Windows.Input;

namespace MeterImportV2.ViewModel.Helpers
{
    public class AsyncRelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        private readonly Func<bool>? canExecute;
        private readonly Func<Task> execute;
        private bool isExecuting;
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }
        public bool CanExecute(object? parameter)
        {
            return !isExecuting && (canExecute?.Invoke() ?? true);
        }

        public async void Execute(object? parameter)
        {
            if (!CanExecute(parameter))
                return;
            try
            {
                isExecuting = true;
                RaiseCanExecuteChanged();
                await execute();
            }
            finally
            {
                isExecuting = false;
                RaiseCanExecuteChanged();
            }
            
        }
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
