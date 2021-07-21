using System.ComponentModel.DataAnnotations;

namespace ChargeBook.viewModels.userViewModels {
    public class RegisterUserViewModel {
        [EmailAddress(ErrorMessage = "Diesen Nutzer kann man nicht registrieren")]
        [Required(ErrorMessage = "Pflichtfeld")]
        public string email { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [StringLength(40, ErrorMessage = "Der Vorname darf max. nur 40 Zeichen lang sein")]
        public string firstName { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [StringLength(40, ErrorMessage = "Der Nachname darf max. nur 40 Zeichen lang sein")]
        public string lastName { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Das Passwort muss zwischen 6 und 100 Zeichen enthalten")]

        public string password { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [Compare(nameof(password), ErrorMessage = "Die Passwörter stimmen nicht überein")]
        public string passwordRepeated { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        public string defaultLocation { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        public string verificationToken { get; set; }

    }
}