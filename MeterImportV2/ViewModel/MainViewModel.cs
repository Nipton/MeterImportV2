using MeterImportV2.Models;
using MeterImportV2.Models.Enums;

namespace MeterImportV2.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string templatePath = string.Empty;
        private string readingsPath = string.Empty;
        private EnumItem<ResourceType> selectedResourceType = null!;
        private EnumItem<Company> selectedCompany = null!;
        private IReadOnlyList<EnumItem<Company>> companies = [];
        private readonly Dictionary<ResourceType, IReadOnlyList<Company>> availableCompanies;
        public string TemplatePath
        {
            get => templatePath;
            set => Set(ref templatePath, value); 
        }
        public string ReadingsPath
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
        

        public MainViewModel()
        {
            availableCompanies = new() { 
                { ResourceType.Electricity, new[] { Company.Dial, Company.Smart, Company.ComfortRule } },
                { ResourceType.ColdWater, new[] { Company.Dial, Company.ComfortRule } } };
            ResourceTypes = Enum.GetValues<ResourceType>().Select(x => new EnumItem<ResourceType>(x)).ToList();
            SelectedResourceType = ResourceTypes.Single(x => x.Value == ResourceType.Electricity);
            
        }
        private void UpdateCompanies()
        {
            var available = availableCompanies[SelectedResourceType.Value];
            Companies = Companies = Enum.GetValues<Company>().Select(x => new EnumItem<Company>(x)).Where(x => available.Contains(x.Value)).ToList();
            SelectedCompany = Companies.First();
        }
    }
}
