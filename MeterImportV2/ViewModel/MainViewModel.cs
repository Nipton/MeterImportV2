using MeterImportV2.Exceptions;
using MeterImportV2.Interfaces;
using MeterImportV2.Models;
using MeterImportV2.Models.Enums;
using MeterImportV2.ViewModel.Helpers;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace MeterImportV2.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private readonly IFileValidator _fileValidator;
        private readonly IImportServiceFactory _factory;
        private bool isProcessing;
        private string? templatePath;
        private string? readingsPath;
        private EnumItem<ResourceType> selectedResourceType = null!;
        private EnumItem<Company> selectedCompany = null!;
        private IReadOnlyList<EnumItem<Company>> companies = [];
        private readonly Dictionary<ResourceType, IReadOnlyList<Company>> availableCompanies;
        public ObservableCollection<ImportMessage> InfoMessages { get; } = new();
        public ObservableCollection<ImportMessage> WarningMessages { get; } = new();
        public ObservableCollection<ImportMessage> ErrorMessages { get; } = new();
        public bool HasInfoMessages => InfoMessages.Any();
        public bool HasWarningMessages => WarningMessages.Any();
        public bool HasErrorMessages => ErrorMessages.Any();
        public event EventHandler? JournalVisibilityChanged;
        public bool IsProcessing
        {
            get => isProcessing;
            set
            {
                if (Set(ref isProcessing, value))
                    ImportCommand.RaiseCanExecuteChanged();
            }
        }
        public string? TemplatePath
        {
            get => templatePath;
            set
            {
                if (Set(ref templatePath, value))
                    ImportCommand.RaiseCanExecuteChanged();
            }
        }
        public string? ReadingsPath
        {
            get => readingsPath;
            set
            {
                if (Set(ref readingsPath, value))
                    ImportCommand.RaiseCanExecuteChanged();
            }
        }
        public EnumItem<ResourceType> SelectedResourceType
        {
            get => selectedResourceType;
            set
            {
                if(Set(ref selectedResourceType, value))
                    UpdateCompanies();
            }
        }
        public EnumItem<Company> SelectedCompany
        {
            get => selectedCompany;
            set => Set(ref selectedCompany, value);
        }
        public IReadOnlyList<EnumItem<Company>> Companies 
        {
            get => companies;
            private set => Set(ref companies, value);
                
        }
        public IReadOnlyList<EnumItem<ResourceType>> ResourceTypes { get; }
        
        public ICommand SelectTemplateCommand { get; }
        public ICommand SelectReadingsCommand { get; }
        public RelayCommand ImportCommand { get; }
        public ICommand CloseLogsCommand { get; }
        public MainViewModel(IDialogService dialogService, IFileValidator fileValidator, IImportServiceFactory factory)
        {
            _dialogService = dialogService;
            _fileValidator = fileValidator;
            _factory = factory;
            availableCompanies = new() { 
                { ResourceType.Electricity, new[] { Company.Dial, Company.Smart, Company.ComfortRule } },
                { ResourceType.ColdWater, new[] { Company.Dial, Company.ComfortRule } } };
            ResourceTypes = Enum.GetValues<ResourceType>().Select(x => new EnumItem<ResourceType>(x)).ToList();
            SelectedResourceType = ResourceTypes.Single(x => x.Value == ResourceType.Electricity);

            SelectTemplateCommand = new RelayCommand(GetTemplatePath);
            SelectReadingsCommand = new RelayCommand(GetReadingsPath);
            ImportCommand = new RelayCommand(Import, CanImport); 
            CloseLogsCommand = new RelayCommand(CloseLogs);
        }
        private void UpdateCompanies()
        {
            var available = availableCompanies[SelectedResourceType.Value];
            Companies = Enum.GetValues<Company>().Select(x => new EnumItem<Company>(x)).Where(x => available.Contains(x.Value)).ToList();
            SelectedCompany = Companies.First();
        }
        private void GetTemplatePath()
        {
            TemplatePath = _dialogService.SelectFile("Выберите шаблон") ?? TemplatePath;
        }
        private void GetReadingsPath()
        {
            ReadingsPath = _dialogService.SelectFile("Выберите файл с показаниями") ?? ReadingsPath;
        }
        private void Import()
        {
            IsProcessing = true;
            CleanLogs();
            try
            {
                if (!ValidateFilePath())
                    return;
                var reader = _factory.CreateReader(SelectedResourceType.Value, SelectedCompany.Value);
                var readerResult = reader.Read(ReadingsPath!);
                if (!readerResult.Readings.Any())
                {
                    _dialogService.ShowWarning("Нет записей в файле с показаниями");
                    return;
                }
                var writer = _factory.CreateWriter();
                var writerMessages = writer.Write(readerResult.Readings, TemplatePath!, SelectedResourceType.Value, SelectedCompany.Value);
                AddMessages(readerResult.ImportMessages);
                AddMessages(writerMessages);
                OnJournalVisibilityChanged();
            }
            catch(ImportException ex)
            {
                _dialogService.ShowError(ex.Message, "Ошибка во время чтения файла с показаниями");
            }
            catch (IOException)
            {
                _dialogService.ShowError("Файл занят или недоступен. Закройте файл и повторите попытку.", "Ошибка доступа к файлу");
            }
            catch (UnauthorizedAccessException)
            {
                _dialogService.ShowError("Нет доступа к файлу. Проверьте права доступа.", "Ошибка доступа");
            }
            catch (Exception)
            {
                _dialogService.ShowError($"Непредвиденная ошибка", "Ошибка");
            }
            finally
            {
                IsProcessing = false;
            }
        }
        private void AddMessages(IEnumerable<ImportMessage> messages)
        {
            foreach (var message in messages)
            {
                switch (message.MessageType)
                {
                    case MessageType.Info:
                        InfoMessages.Add(message);
                        break;
                    case MessageType.Warning:
                        WarningMessages.Add(message);
                        break;
                    case MessageType.Error:
                        ErrorMessages.Add(message);
                        break;
                }
            }
            NotifyMessageVisibilityChanged();
        }
        private bool CanImport()
        {
            return !IsProcessing && !string.IsNullOrWhiteSpace(ReadingsPath) && !string.IsNullOrWhiteSpace(TemplatePath);
        }
        private bool ValidateFilePath()
        {
            var templateValid = _fileValidator.ValidateFilePath("Шаблон", TemplatePath);
            if (!templateValid.IsValid)
            {
                _dialogService.ShowWarning(templateValid.Message);
                return false;
            }
            var readingsValid = _fileValidator.ValidateFilePath("Файл показаний", ReadingsPath);
            if (!readingsValid.IsValid)
            {
                _dialogService.ShowWarning(readingsValid.Message);
                return false;
            }
            return true;
        }
        private void CleanLogs()
        {
            InfoMessages.Clear();
            WarningMessages.Clear();
            ErrorMessages.Clear();

            NotifyMessageVisibilityChanged();
        }
        private void CloseLogs()
        {
            CleanLogs();
            OnJournalVisibilityChanged();
        }
        private void NotifyMessageVisibilityChanged()
        {
            OnPropertyChanged(nameof(HasInfoMessages));
            OnPropertyChanged(nameof(HasWarningMessages));
            OnPropertyChanged(nameof(HasErrorMessages));
        }
        private void OnJournalVisibilityChanged()
        {
            JournalVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
