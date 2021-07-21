using System.ComponentModel.DataAnnotations;

namespace ChargeBook.viewModels.userViewModels {
    public class ResetPasswordViewModel {
        [Required(ErrorMessage = "Pflichtfeld")]
        [EmailAddress(ErrorMessage = "Diese E-Mailadresse existiert nicht im System")]
        public string email { get; set; }

        [Required(ErrorMessage = "Der Token des Passwort Zurücksetzen ist nicht vorhanden")]
        public string resetPasswordToken { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Das Passwort muss zwischen 6 und 100 Zeichen enthalten")]
        public string password { get; set; }

        [Required(ErrorMessage = "Pflichtfeld")]
        [Compare(nameof(password), ErrorMessage = "Die Passwörter stimmen nicht überein")]
        public string passwordRepeated { get; set; }
    }
}