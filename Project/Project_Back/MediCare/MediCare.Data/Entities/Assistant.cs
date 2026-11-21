
namespace MediCare.Data.Entities
{
    public class Assistant
    {
        public string Id { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;
    }
}
