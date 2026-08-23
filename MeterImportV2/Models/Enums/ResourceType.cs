using System.ComponentModel.DataAnnotations;

namespace MeterImportV2.Models.Enums
{
    public enum ResourceType
    {
        [Display(Name = "Электроэнергия")]
        Electricity,
        [Display(Name = "ХВС")]
        ColdWater
    }
}
