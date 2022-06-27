namespace belong_challenge.Models
{
    public class Slot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get { return StartTime + TimeSpan.FromMinutes(30); } }

        public Slot(DateTime startTime)
        {
            StartTime = startTime;
        }
    }
}
