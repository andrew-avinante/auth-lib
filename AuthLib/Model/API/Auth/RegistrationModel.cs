using System.ComponentModel.DataAnnotations;

namespace AuthLib.Model.API.Auth
{
    public class RegistrationModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
        [PasswordConfirmed]
        [Required(ErrorMessage = "Confirm password is required")]
        public string? ConfirmPassword { get; set; }
    }

    class PasswordConfirmed : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            RegistrationModel registrationModel = (RegistrationModel)validationContext.ObjectInstance;

            if(registrationModel.Password == registrationModel.ConfirmPassword)
            {
                return null;
            }

            return new ValidationResult("Passwords do not match");
        }
    }
}
