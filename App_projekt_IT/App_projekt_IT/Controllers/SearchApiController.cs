using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App_projekt_IT.Data;


namespace App_projekt_IT.Controllers
{
    [Route("api/[controller]")]
    [ApiController] 
    public class SearchApiController : ControllerBase 
    {
        private readonly ApplicationDbContext _context;

        public SearchApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Metody
        // GET: 
        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            
            var cities = await _context.Cities
                .Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(cities); 
        }

        // GET: 
        [HttpGet("services")]
        public async Task<IActionResult> GetServices()
        {
            var services = await _context.Services
                .Select(s => new
                {
                    Id = s.Id,
                    Name = s.Name,
                    IsNFZ = s.IsNFZ
                })
                .ToListAsync();

            return Ok(services);
        }

        // GET: 
        [HttpGet("slots")]
        public async Task<IActionResult> GetAvailableSlots(int clinicId, int serviceId, DateTime date)
        {
            // Filtrowanie na poziomie bazy danych 
            var availableSlots = await _context.AppointmentSlots
                .Include(a => a.Doctor) 
                .Where(a =>
                    a.ServiceId == serviceId &&
                    a.Doctor.ClinicId == clinicId && 
                    a.StartTime.Date == date.Date && 
                    a.IsBooked == false) 
                .OrderBy(a => a.StartTime) 
                .Select(a => new
                {
                    SlotId = a.Id,
                    Time = a.StartTime.ToString("HH:mm"), 
                    DoctorName = $"{a.Doctor.FirstName} {a.Doctor.LastName}"
                })
                .ToListAsync();

            if (!availableSlots.Any())
            {
                return NotFound(new { message = "Brak wolnych terminów na wybrany dzień." }); 
            }

            return Ok(availableSlots); 
        }
    }
}
