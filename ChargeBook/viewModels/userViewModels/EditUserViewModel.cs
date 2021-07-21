using System.ComponentModel.DataAnnotations;
using ChargeBook.attributes.validationAttributes;

namespace ChargeBook.viewModels.userViewModels {
    public class EditUserViewModel {
        [Required(ErrorMessage = "Pflichtfeld")]
        [EmailAddress(ErrorMessage = "Diesen Nutzer kann man nicht editieren")]
        public string email { get; set; }

        [Required(ErrorMessage = "Es muss angegeben werden, ob der Nutzer kein/ein Admin ist")]
        public bool isAdmin { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [PriorityRoleExitsValidation(ErrorMessage = "Die Rolle existiert nicht im System")]
        public string priorityRole { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [StringLength(40, ErrorMessage = "Der Vorname darf maximal nur 40 Zeichen lang sein")]
        public string firstName { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [StringLength(40, ErrorMessage = "Der Nachname darf maximal nur 40 Zeichen lang sein")]
        public string lastName { get; set; }
    }
}