namespace MediCare.Services.ViewModels
{
    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool IsPendingDoctor { get; set; }
        public bool IsRejectedDoctor { get; set; }
        public string? RejectionReason { get; set; }

        public static AuthResult Success()
        {
            return new AuthResult { Succeeded = true };
        }

        public static AuthResult Failed(params string[] errors)
        {
            return new AuthResult
            {
                Succeeded = false,
                Errors = errors.ToList()
            };
        }

        public static AuthResult Failed(IEnumerable<string> errors)
        {
            return new AuthResult
            {
                Succeeded = false,
                Errors = errors.ToList()
            };
        }

        public static AuthResult PendingDoctor()
        {
            return new AuthResult
            {
                Succeeded = false,
                IsPendingDoctor = true,
                Errors = new List<string>
                {
                    "Your doctor account is pending approval by the clinic administration / حسابك كطبيب لا يزال قيد المراجعة والاعتماد من قبل إدارة العيادة."
                }
            };
        }

        public static AuthResult RejectedDoctor(string? reason)
        {
            var msg = string.IsNullOrWhiteSpace(reason)
                ? "Your doctor account has been rejected by the clinic administration / تم رفض حسابك كطبيب من قبل إدارة العيادة."
                : $"Your doctor account has been rejected. Reason: {reason} / تم رفض حسابك كطبيب من قبل إدارة العيادة. السبب: {reason}";

            return new AuthResult
            {
                Succeeded = false,
                IsRejectedDoctor = true,
                RejectionReason = reason,
                Errors = new List<string> { msg }
            };
        }
    }
}
