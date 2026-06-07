using App_projekt_IT.Data;
using App_projekt_IT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace App_projekt_IT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        
        private readonly ApplicationDbContext _context;

        
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            
            var topReviews = await _context.Reviews
                .Include(r => r.AppointmentSlot)
                    .ThenInclude(a => a.Doctor)
                .Where(r => r.Rating >= 4 && !string.IsNullOrEmpty(r.Comment))
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.TopReviews = topReviews;

            return View();
        }

        public async Task<IActionResult> Search(int? serviceId, int? cityId, DateTime? appointmentDate, string payment)
        {
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", serviceId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", cityId);

            TimeZoneInfo polishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
            DateTime currentPolishTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, polishTimeZone);

            var query = _context.AppointmentSlots
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Clinic)
                        .ThenInclude(c => c.City)
                .Include(a => a.Service)
                .Where(a => a.IsBooked == false && a.StartTime > currentPolishTime);

            bool hasSearched = serviceId.HasValue || cityId.HasValue || appointmentDate.HasValue || !string.IsNullOrEmpty(payment);

            if (hasSearched)
            {
                if (serviceId.HasValue)
                    query = query.Where(a => a.ServiceId == serviceId.Value);

                if (cityId.HasValue)
                    query = query.Where(a => a.Doctor.Clinic.CityId == cityId.Value);

                if (appointmentDate.HasValue)
                {
                    var startOfDay = appointmentDate.Value.Date;
                    var endOfDay = startOfDay.AddDays(1);
                    query = query.Where(a => a.StartTime >= startOfDay && a.StartTime < endOfDay);
                }

                if (payment == "NFZ")
                    query = query.Where(a => a.Service.IsNFZ == true);
                else if (payment == "Prywatnie")
                    query = query.Where(a => a.Service.IsNFZ == false);

                var slots = await query.OrderBy(a => a.StartTime).ToListAsync();

                
                List<AppointmentSlot> alternativeSlots = new List<AppointmentSlot>();

                
                if (!slots.Any() && appointmentDate.HasValue)
                {
                    
                    var altQuery = _context.AppointmentSlots
                        .Include(a => a.Doctor)
                            .ThenInclude(d => d.Clinic)
                                .ThenInclude(c => c.City)
                        .Include(a => a.Service)
                        .Where(a => a.IsBooked == false && a.StartTime > currentPolishTime);

                    if (serviceId.HasValue) altQuery = altQuery.Where(a => a.ServiceId == serviceId.Value);
                    if (cityId.HasValue) altQuery = altQuery.Where(a => a.Doctor.Clinic.CityId == cityId.Value);
                    if (payment == "NFZ") altQuery = altQuery.Where(a => a.Service.IsNFZ == true);
                    else if (payment == "Prywatnie") altQuery = altQuery.Where(a => a.Service.IsNFZ == false);

                   
                    var nearestSlot = await altQuery.OrderBy(a => a.StartTime).FirstOrDefaultAsync();

                    if (nearestSlot != null)
                    {
                        
                        var altDayStart = nearestSlot.StartTime.Date;
                        var altDayEnd = altDayStart.AddDays(1);

                        alternativeSlots = await altQuery
                            .Where(a => a.DoctorId == nearestSlot.DoctorId && a.StartTime >= altDayStart && a.StartTime < altDayEnd)
                            .OrderBy(a => a.StartTime)
                            .ToListAsync();
                    }
                }

                ViewBag.AlternativeSlots = alternativeSlots;
                
                var doctorIds = slots.Select(a => a.DoctorId).ToList();
                doctorIds.AddRange(alternativeSlots.Select(a => a.DoctorId));
                doctorIds = doctorIds.Distinct().ToList();

                ViewBag.DoctorRatings = await _context.Reviews
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

                return View(slots);
            }

            return View(new List<AppointmentSlot>());
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}