namespace MediCare.Data.Entities
{
    public class HomeAddress
    {
        public int Id { get; set; }

        public string City { get; set; } = string.Empty;

        public string Street { get; set; } = string.Empty;
    }
}