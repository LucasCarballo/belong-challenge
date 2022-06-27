namespace belong_challenge.Models
{
    public class Tour
    {
        public int Id { get; set; }
        public string HomeId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string UserId { get; set; }
        public bool Cancelled { get; set; } = false;
        public bool Rescheduled { get; set; } = false;

        public Tour(string homeId, DateTime scheduledAt, string userId)
        {
            HomeId = homeId;
            ScheduledAt = scheduledAt;
            UserId = userId;
        }
    }
}
