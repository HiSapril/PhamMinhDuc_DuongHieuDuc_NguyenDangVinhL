using System.ComponentModel.DataAnnotations;


namespace ASCWeb1.Areas.Accounts.Models
{
    public class ServiceEngineerRegistrationViewModel : IValidatableObject
    {
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool IsEdit { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Password chỉ bắt buộc khi tạo mới (IsEdit = false)
            if (!IsEdit && string.IsNullOrWhiteSpace(Password))
            {
                yield return new ValidationResult(
                    "The Password field is required.",
                    new[] { nameof(Password) }
                );
            }

            // Validate password length nếu có nhập
            if (!string.IsNullOrWhiteSpace(Password) && (Password.Length < 6 || Password.Length > 100))
            {
                yield return new ValidationResult(
                    "The Password must be at least 6 and at max 100 characters long.",
                    new[] { nameof(Password) }
                );
            }
        }
    }
}
