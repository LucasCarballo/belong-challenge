using belong_challenge.Exceptions;
using belong_challenge.Models;
using belong_challenge.Models.DTO;
using belong_challenge.Repositories;

namespace belong_challenge.Services
{
    public class TourService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TourRepository _tourRepository;

        public TourService(IHttpClientFactory httpClientFactory, TourRepository tourRepository)
        {
            _httpClientFactory = httpClientFactory;
            _tourRepository = tourRepository;
        }

        internal async Task<List<Slot>> GetAvailableSlots(string homeId)
        {
            if (!(bool)await IsSelfServeTourAvailable(homeId))
            {
                throw new TourException("Home is not available to self serve tours");
            }

            return GetHomeAvailableSlots(homeId);
        }

        internal async Task<Tour> Book(string homeId, DateTime tourTime, string userId)
        {
            ValidateTourTime(tourTime);

            if (!(bool)await IsSelfServeTourAvailable(homeId))
            {
                throw new TourException("Home is not available to self serve tours");
            }

            if (!IsSlotAvailable(homeId, tourTime))
            {
                throw new TourException("Tour slot is not available");
            }

            return await _tourRepository.Insert(new Tour(homeId, tourTime, userId));
        }

        internal void Cancel(int tourId)
        {
            var tour = _tourRepository.Get(tourId);
            if (tour == null)
            {
                throw new TourException("Tour unavailable to cancel");
            }

            if (tour.ScheduledAt <= DateTime.Now)
            {
                throw new TourException("Tour unavailable to cancel");
            }

            _tourRepository.Cancel(tourId);
        }

        internal async Task<Tour> Reschedule(int tourId, DateTime tourTime)
        {
            var tour = _tourRepository.Get(tourId);

            if (tour == null)
            {
                throw new TourException("Tour unavailable to reschedule");
            }

            if (tour.ScheduledAt <= DateTime.Now)
            {
                throw new TourException("Tour unavailable to reschedule");
            }

            ValidateTourTime(tourTime);

            if (!IsSlotAvailable(tour.HomeId, tourTime))
            {
                throw new TourException("Tour slot is not available");
            }

            var rescheduledTour = await _tourRepository.Reschedule(tourId);

            if (rescheduledTour == null)
            {
                throw new TourException("Cannot find tourId to reschedule");
            }

            return await _tourRepository.Insert(new Tour(rescheduledTour.HomeId, tourTime, rescheduledTour.UserId));
        }

        internal StatsDTO GetStats()
        {
            var booked = _tourRepository.GetBooked();
            var cancelled = _tourRepository.GetCancelled();
            var rescheduled = _tourRepository.GetRescheduled();

            return new StatsDTO(booked.Count, cancelled.Count, rescheduled.Count);
        }

        private void ValidateTourTime(DateTime tourTime)
        {
            var now = DateTime.Now;
            if (now.Hour >= 21 && tourTime.Date == (now + TimeSpan.FromDays(1)).Date)
            {
                throw new TourException("Cannot schedule a tour for tomorrow after 9.00pm");
            }

            if (now.Date == tourTime.Date)
            {
                throw new TourException("Cannot schedule a tour for the current day");
            }
        }

        private bool IsSlotAvailable(string homeId, DateTime tourTime)
        {
            var availableSlots = GetHomeAvailableSlots(homeId);
            return availableSlots.Any(s => s.StartTime == tourTime);
        }

        private List<Slot> GetHomeAvailableSlots(string homeId)
        {
            var tours = _tourRepository.GetUpcomingTours(homeId);
            var availableSlots = BuildAllAvailableSlots();
            if (tours == null)
            {
                return availableSlots;
            }

            foreach (var tour in tours)
            {
                var tourSlot = new Slot(tour.ScheduledAt);
                availableSlots.RemoveAll(s =>
                    s.EndTime.Equals(tourSlot.StartTime) ||
                    s.StartTime.Equals(tourSlot.StartTime) || 
                    s.StartTime.Equals(tourSlot.EndTime));
            }

            return availableSlots;
        }

        private List<Slot> BuildAllAvailableSlots()
        {
            var tomorrow = GetNextWorkingDay(1);
            var dayAfterTomorrow = GetNextWorkingDay(2);
            var twoDaysInFuture = GetNextWorkingDay(3);
            
            var availableSlots = new List<Slot>();
            availableSlots.AddRange(BuildAvailableSlotsPerDay(tomorrow));
            availableSlots.AddRange(BuildAvailableSlotsPerDay(dayAfterTomorrow));
            availableSlots.AddRange(BuildAvailableSlotsPerDay(twoDaysInFuture));

            return availableSlots;
        }

        private DateTime GetNextWorkingDay(int fromDays)
        {
            var nextWorkingDay = DateTime.Now + TimeSpan.FromDays(fromDays);

            if (nextWorkingDay.DayOfWeek == DayOfWeek.Saturday || nextWorkingDay.DayOfWeek == DayOfWeek.Sunday)
            {
                return GetNextWorkingDay(fromDays + 1);
            }
            else
            {
                return nextWorkingDay;
            }
        }

        private List<Slot> BuildAvailableSlotsPerDay(DateTime dateTime)
        {
            var firstSlotStart = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 10, 00, 00);
            var lastSlotStart = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 16, 30, 00);

            var result = new List<Slot>();
            for (var i = firstSlotStart; i <= lastSlotStart; i += TimeSpan.FromMinutes(30))
            {
                result.Add(new Slot(i));
            }

            return result;
        }

        private async Task<bool?> IsSelfServeTourAvailable(string homeId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var httpResponse = await httpClient.GetAsync($"https://api-production.bln.hm/homes/{homeId}");

            var response = await httpResponse.Content.ReadFromJsonAsync<HomeResponse>();
            //return response?.ListingInfo?.IsSelfServeVisitsAllowed;
            return BooleanRandomlyGenerated();
        }

        private static bool? BooleanRandomlyGenerated()
        {
            var rand = new Random();
            return rand.NextDouble() >= 0.5;
        }
    }
}
