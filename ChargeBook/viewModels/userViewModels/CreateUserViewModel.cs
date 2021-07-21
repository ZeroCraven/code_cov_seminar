using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace ChargeBook.viewModels.userViewModels {
    public class CreateUserViewModel {
        [Required(ErrorMessage = "Pflichtfeld")]
        [EmailAddress(ErrorMessage = "Diesen Nutzer kann man nicht erstellen")]
        public string email { get; set; }

        [Required(ErrorMessage = "Es muss angegeben werden, ob der Nutzer kein/ein Admin ist")]
        public bool isAdmin { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [PriorityRoleExitsValidation(ErrorMessage = "Die angegebene Rolle existiert nicht im System")]
        public string priorityRole { get; set; }
    }
}