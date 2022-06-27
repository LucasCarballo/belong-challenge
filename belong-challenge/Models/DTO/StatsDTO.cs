namespace belong_challenge.Models.DTO
{
    public class StatsDTO
    {
        public int Booked { get; set; }
        public int Cancelled { get; set; }
        public int Rescheduled { get; set; }

        public StatsDTO(int booked, int cancelled, int rescheduled)
        {
            Booked = booked;
            Cancelled = cancelled;
            Rescheduled = rescheduled;
        }
    }
}
