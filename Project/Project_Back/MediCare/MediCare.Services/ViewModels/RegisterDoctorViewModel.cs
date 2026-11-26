using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.ViewModels
{
    public class RegisterDoctorViewModel
    {
        [Required(ErrorMessage = "First name is required / الاسم الأول مطلوب")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters / الاسم بين 2 و50 حرفاً")]
        [RegularExpression(@"^[a-zA-Z\u0621-\u064A\s\-']+$", ErrorMessage = "First name can only contain letters / الاسم يجب أن يحتوي على حروف فقط")]
        [Display(Name = "First Name / الاسم الأول")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required / اسم العائلة مطلوب")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters / اسم العائلة بين 2 و50 حرفاً")]
        [RegularExpression(@"^[a-zA-Z\u0621-\u064A\s\-']+$", ErrorMessage = "Last name can only contain letters / اسم العائلة يجب أن يحتوي على حروف فقط")]
        [Display(Name = "Last Name / اسم العائلة")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required / البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "Invalid email format / صيغة البريد غير صحيحة")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        [Display(Name = "Email Address / البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required / رقم الهاتف مطلوب")]
        [RegularExpression(@"^(\+20|0)?1[0125]\d{8}$",
            ErrorMessage = "Enter a valid Egyptian phone number (e.g. 01012345678) / أدخل رقم هاتف مصري صحيح")]
        [Display(Name = "Phone Number / رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gender is required / الجنس مطلوب")]
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Invalid gender value / قيمة الجنس غير صحيحة")]
        [Display(Name = "Gender / الجنس")]
        public string Gender { get; set; } = "Male";

        [Required(ErrorMessage = "Specialization is required / التخصص الطبي مطلوب")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a specialization / اختر تخصصاً طبياً")]
        [Display(Name = "Specialization / التخصص")]
        public int SpecializationId { get; set; }

        [Required(ErrorMessage = "Consultation fee is required / سعر الكشف مطلوب")]
        [Range(50, 50000, ErrorMessage = "Fee must be between 50 and 50,000 EGP / سعر الكشف بين 50 و50,000 جنيه")]
        [Display(Name = "Consultation Fee (EGP) / سعر الكشف (جنيه)")]
        public decimal ConsultationFee { get; set; }

        [Required(ErrorMessage = "City is required / المدينة مطلوبة")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters / المدينة بين 2 و100 حرف")]
        [Display(Name = "City / المدينة")]
        public string City { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters / العنوان لا يتجاوز 250 حرفاً")]
        [Display(Name = "Clinic Address / عنوان العيادة")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Password is required / كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be between 8 and 100 characters / كلمة المرور بين 8 و100 حرف")]
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
