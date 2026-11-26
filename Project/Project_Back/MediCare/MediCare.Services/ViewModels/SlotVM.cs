namespace MediCare.Services.ViewModels
{
    /// <summary>
    /// One free time slot of a doctor on a given day.
    /// Start / End are full date-times so FullCalendar can use them directly.
    /// </summary>
    public class SlotVM
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}
