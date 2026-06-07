using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App_projekt_IT.Data;
using System.Linq;


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

            var doctorIds = availableSlots.Select(a => a.DoctorId).Distinct().ToList();

            // Pobieramy opinie tylko dla tych lekarzy, grupujemy i wyliczamy średnią
            var doctorRatings = await _context.Reviews
                .Include(r => r.AppointmentSlot)
                .Where(r => doctorIds.Contains(r.AppointmentSlot.DoctorId))
                .GroupBy(r => r.AppointmentSlot.DoctorId)
                .Select(g => new
                {
                    DoctorId = g.Key,
                    AverageRating = Math.Round(g.Average(r => r.Rating), 1),
                    ReviewCount = g.Count()
                })
                .ToDictionaryAsync(k => k.DoctorId, v => v);



            var groupedResults = availableSlots
                .GroupBy(a => new
                {
                    DoctorId = a.DoctorId,
                    ClinicName = a.Doctor.Clinic.Name, 
                    DoctorFirstName = a.Doctor.FirstName,
                    DoctorLastName = a.Doctor.LastName
                })
                .Select(g => 
                {
                    bool hasRatings = doctorRatings.ContainsKey(g.Key.DoctorId);

                    return new
                    {
                        clinicName = g.Key.ClinicName,
                        doctorName = $"Dr {g.Key.DoctorFirstName} {g.Key.DoctorLastName}",

                        rating = hasRatings ? doctorRatings[g.Key.DoctorId].AverageRating : (double?)null,
                        reviewCount = hasRatings ? doctorRatings[g.Key.DoctorId].ReviewCount : 0,
                        availableSlots = g.Select(s => new
                        {
                            slotId = s.Id,
                            time = s.StartTime.ToString("HH:mm")
                        }).ToList()
                    };
                    
                })
                .ToList();

            return Ok(groupedResults); 
        }
    }
}
