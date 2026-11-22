using System.ComponentModel.DataAnnotations;

namespace SeventWebApplication.Models.Users
{
    // Det som skickas in från registreringssidan. Fälten valideras innan något når AuthService,
    // så felen visas vid rätt fält. Reglerna för e-post och telefon delas med User via ContactValidator.
    public class RegisterForm : IValidatableObject
    {
        [Required(ErrorMessage = "Enter your name.")]
        [StringLength(100, ErrorMessage = "The name can be at most 100 characters.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Enter your email address.")]
        public string Mail { get; set; } = "";

        public string? Phone { get; set; }

        [Required(ErrorMessage = "Choose a password.")]
        [MinLength(8, ErrorMessage = "The password must be at least 8 characters.")]
        public string Password { get; set; } = "";

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(Mail) && !ContactValidator.IsValidMail(Mail))
                yield return new ValidationResult(ContactValidator.InvalidMailMessage, new[] { nameof(Mail) });

            if (!ContactValidator.IsValidPhone(Phone))
                yield return new ValidationResult(ContactValidator.InvalidPhoneMessage, new[] { nameof(Phone) });
        }
    }
}
