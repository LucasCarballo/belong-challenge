using belong_challenge.Database;
using belong_challenge.Models;

namespace belong_challenge.Repositories
{
    public class TourRepository
    {
        private readonly TourContext _context;

        public TourRepository(TourContext context)
        {
            _context = context;
        }

        public Tour? Get(int tourId)
        {
            return _context.Tours.FirstOrDefault(t => 
                t.Id.Equals(tourId) && 
                !t.Cancelled && 
                !t.Rescheduled && 
                t.ScheduledAt >= DateTime.Now);
        }

        public List<Tour> GetUpcomingTours(string homeId)
        {
            return _context.Tours.Where(t => 
                t.HomeId.Equals(homeId) &&
                !t.Cancelled &&
                !t.Rescheduled &&
                t.ScheduledAt >= DateTime.Now
            ).ToList();
        }

        internal async Task<Tour> Insert(Tour tour)
        {
            var entityEntry = _context.Add(tour);
            await _context.SaveChangesAsync();

            return entityEntry.Entity;
        }

        internal async void Cancel(int tourId)
        {
            var entity = _context.Tours.FirstOrDefault(t => t.Id.Equals(tourId));

            if (entity != null)
            {
                entity.Cancelled = true;
                await _context.SaveChangesAsync();
            }
        }

        internal async Task<Tour?> Reschedule(int tourId)
        {
            var entity = _context.Tours.FirstOrDefault(t => t.Id.Equals(tourId));

            if (entity != null)
            {
                entity.Rescheduled = true;
                await _context.SaveChangesAsync();
            }

            return entity;
        }

        internal List<Tour> GetBooked()
        {
            return _context.Tours.Where(t => !t.Cancelled && !t.Rescheduled).ToList();
        }

        internal List<Tour> GetCancelled()
        {
            return _context.Tours.Where(t => t.Cancelled).ToList();
        }

        internal List<Tour> GetRescheduled()
        {
            return _context.Tours.Where(t => t.Rescheduled).ToList();
        }
    }
}
