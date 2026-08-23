using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeterImportV2.Models.Enums
{
    public enum Company
    {
        [Display(Name = "Диал")]
        Dial,
        [Display(Name = "Смарт")]
        Smart,
        [Display(Name = "Правило комфорта")]
        ComfortRule
    }
}
