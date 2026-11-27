namespace MediCare.Services.ViewModels
{
    /// <summary>One option of the specialization dropdown (value = Id, text = Name).</summary>
    public class SpecializationOptionVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}