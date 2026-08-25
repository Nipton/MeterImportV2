using MeterImportV2.Exceptions;
using MeterImportV2.Interfaces;
using MeterImportV2.Models.Enums;
using MeterImportV2.ViewModel.Helpers;
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
        public string? TemplatePath
        {
            get => templatePath;
            set => Set(ref templatePath, value); 
        }
        public string? ReadingsPath
        {
            get => readingsPath;
            set => Set(ref readingsPath, value);
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
        public ICommand ImportCommand { get; }
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
            ImportCommand = new RelayCommand(Import); 
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
            isProcessing = true;
            try
            {
                if (!ValidateFilePath())
                    return;
                var reader = _factory.CreateReader(SelectedResourceType.Value, SelectedCompany.Value);
                var readerResult = reader.Read(ReadingsPath!);
            }
            catch(ReaderException ex)
            {
                _dialogService.ShowError(ex.Message, "Ошибка во время чтения файла с показаниями");
            }
            finally
            {
                isProcessing = false;
            }
        }
        private bool CanImport()
        {
            return !isProcessing && !string.IsNullOrWhiteSpace(ReadingsPath) && !string.IsNullOrWhiteSpace(TemplatePath) && SelectedResourceType != null && SelectedCompany != null;
        }
        private bool ValidateFilePath()
        {
            var templateValid = _fileValidator.ValidateFilePath("Шаблон", TemplatePath);
            var readingsValid = _fileValidator.ValidateFilePath("Файл показаний", ReadingsPath);
            if (!templateValid.IsValid)
            {
                _dialogService.ShowWarning(templateValid.Message);
                return false;
            }
            if (!readingsValid.IsValid)
            {
                _dialogService.ShowWarning(readingsValid.Message);
                return false;
            }
            return true;
        }
    }
}
