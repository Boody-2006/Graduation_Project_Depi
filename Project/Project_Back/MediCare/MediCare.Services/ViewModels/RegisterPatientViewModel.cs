using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.ViewModels
{
    public class RegisterPatientViewModel
    {
        [Required(ErrorMessage = "First name is required / الاسم الأول مطلوب")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters / الاسم لا يمكن أن يتجاوز 50 حرفاً")]
        [Display(Name = "First Name / الاسم الأول")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required / اسم العائلة مطلوب")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters / اسم العائلة لا يمكن أن يتجاوز 50 حرفاً")]
        [Display(Name = "Last Name / اسم العائلة")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required / البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "Invalid email format / صيغة البريد غير صحيحة")]
        [Display(Name = "Email Address / البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required / رقم الهاتف مطلوب")]
        [Phone(ErrorMessage = "Invalid phone number / رقم الهاتف غير صالح")]
        [Display(Name = "Phone Number / رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required / الجنس مطلوب")]
        [Display(Name = "Gender / الجنس")]
        public string Gender { get; set; } = "Male";

        [Required(ErrorMessage = "Date of birth is required / تاريخ الميلاد مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth / تاريخ الميلاد")]
        public DateTime DateOfBirth { get; set; } = DateTime.Today.AddYears(-20);

        [Required(ErrorMessage = "Password is required / كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters / كلمة المرور يجب ألا تقل عن 6 أحرف")]
        [DataType(DataType.Password)]
        [Display(Name = "Password / كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm password / يرجى تأكيد كلمة المرور")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password / تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "Passwords do not match / كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
