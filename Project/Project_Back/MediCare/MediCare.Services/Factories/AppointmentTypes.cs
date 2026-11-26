namespace MediCare.Services.Factories
{
    /// <summary>The appointment types the system can create.</summary>
    public static class AppointmentTypes
    {
        public const string Consultation = "Consultation";
        public const string FollowUp = "FollowUp";

        public static bool IsValid(string? type)
        {
            return type == Consultation || type == FollowUp;
        }
    }
}
