using System.ComponentModel.DataAnnotations;

namespace chargebook.models {
    public enum ConnectorType {
        [Display(Name = "CCS")] CCS,
        [Display(Name = "Type 2")] TYPE_TWO,
        [Display(Name = "Type 1")] TYPE_ONE,
        [Display(Name = "CHAdeMO")] CHADEMO,
        [Display(Name = "Schuko")] SCHUKO,
        [Display(Name = "Tesla Supercharger")] TESLA_SUPERCHARGER
    }
}