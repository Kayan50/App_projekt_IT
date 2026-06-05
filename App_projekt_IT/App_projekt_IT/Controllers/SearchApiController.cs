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
        // GET: api/SearchApi/cities
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

        // GET: api/SearchApi/services
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
        public async Task<IActionResult> GetAvailableSlots(int cityId, int serviceId, DateTime date, bool isNfz)
        {
            
            var availableSlots = await _context.AppointmentSlots
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Clinic)
                .Include(a => a.Service) 
                .Where(a =>
                    a.Doctor.Clinic.CityId == cityId && 
                    a.ServiceId == serviceId &&
                    a.Service.IsNFZ == isNfz && 
                    a.StartTime.Date == date.Date &&
                    a.IsBooked == false)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            if (!availableSlots.Any())
            {
                
                return Ok(new List<object>());
            }

            var groupedResults = availableSlots
                .GroupBy(a => new
                {
                    ClinicName = a.Doctor.Clinic.Name, 
                    DoctorFirstName = a.Doctor.FirstName,
                    DoctorLastName = a.Doctor.LastName
                })
                .Select(g => new
                {
                    clinicName = g.Key.ClinicName,
                    doctorName = $"Dr {g.Key.DoctorFirstName} {g.Key.DoctorLastName}",
                    availableSlots = g.Select(s => new
                    {
                        slotId = s.Id,
                        time = s.StartTime.ToString("HH:mm")
                    }).ToList()
                })
                .ToList();

            return Ok(groupedResults); 
        }
    }
}
