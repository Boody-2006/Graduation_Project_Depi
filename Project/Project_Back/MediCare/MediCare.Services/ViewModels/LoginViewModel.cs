using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required / البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "Invalid email format / صيغة البريد غير صحيحة")]
        [Display(Name = "Email Address / البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required / كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "Password / كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me / تذكرني")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
