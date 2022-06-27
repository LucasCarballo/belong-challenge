using belong_challenge.Exceptions;
using belong_challenge.Models;
using belong_challenge.Models.DTO;
using belong_challenge.Services;
using Microsoft.AspNetCore.Mvc;

namespace belong_challenge.Controllers
{
    [ApiController]
    [Route("tour")]
    public class TourController : ControllerBase
    {
        private readonly ILogger<TourController> _logger;
        private readonly TourService _tourService;

        public TourController(ILogger<TourController> logger, TourService tourService)
        {
            _logger = logger;
            _tourService = tourService;
        }

        [HttpGet("slots/{homeId}", Name = "GetAvailableSlots")]
        public async Task<IActionResult> GetAvailableSlots(string homeId)
        {
            try
            {
                return Ok(await _tourService.GetAvailableSlots(homeId));
            }
            catch (TourException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost(Name = "BookTour")]
        public async Task<IActionResult> BookTour(BookTourDTO bookTour)
        {
            try
            {
                return Ok(await _tourService.Book(bookTour.HomeId, bookTour.TourTime, bookTour.UserId));
            }
            catch (TourException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{tourId}", Name = "CancelTour")]
        public IActionResult CancelTour(int tourId)
        {
            try
            {
                _tourService.Cancel(tourId);
                return Ok();
            }
            catch (TourException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{tourId}/reschedule", Name = "RescheduleTour")]
        public async Task<IActionResult> RescheduleTour(int tourId, DateTime tourTime) 
        {
            try
            {
                return Ok(await _tourService.Reschedule(tourId, tourTime));
            }
            catch (TourException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("stats", Name = "GetStats")]
        public StatsDTO GetStats()
        {
            return _tourService.GetStats();
        }
    }
}
