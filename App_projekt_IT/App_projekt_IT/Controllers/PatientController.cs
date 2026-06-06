using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using App_projekt_IT.Data;
using App_projekt_IT.Models;

namespace App_projekt_IT.Controllers
{
    
    [Authorize]
    public class PatientPanelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientPanelController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET
        public async Task<IActionResult> Index()
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            var userAppointments = await _context.AppointmentSlots
                .Include(a => a.Doctor)    
                .Include(a => a.Service)   
                .Where(a => a.UserId == userId && a.IsBooked == true)
                .OrderBy(a => a.StartTime) 
                .ToListAsync();

            return View(userAppointments);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           
            var appointment = await _context.AppointmentSlots
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (appointment != null)
            {

                var appointmentDate = appointment.StartTime.ToString("dd.MM.yyyy HH:mm");

                
                appointment.IsBooked = false;
                appointment.UserId = null;
                appointment.IsConfirmed = false; 

                
                var notification = new App_projekt_IT.Models.Notification
                {
                    UserId = userId,
                    AppointmentSlotId = null, 
                    Message = $"Twoja wizyta zaplanowana na {appointmentDate} została pomyślnie odwołana.",
                    Type = "Anulowano" 
                };

                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
            }

            
            return RedirectToAction(nameof(Index));
        }
    }
}