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


        public async Task<IActionResult> Index(int? serviceId, int? cityId, DateTime? appointmentDate, string payment)
        {
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", serviceId);
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", cityId);

            var query = _context.AppointmentSlots
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Clinic)
                        .ThenInclude(c => c.City)
                .Include(a => a.Service)
                .Where(a => a.IsBooked == false && a.StartTime > DateTime.Now);

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