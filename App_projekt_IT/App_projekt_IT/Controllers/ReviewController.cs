using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App_projekt_IT.Data;
using App_projekt_IT.Models;
using System.Security.Claims;

namespace App_projekt_IT.Controllers
{
    [Authorize] 
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: 
        public async Task<IActionResult> Create(int appointmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            var appointment = await _context.AppointmentSlots
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

            if (appointment == null) return NotFound();

            if (appointment.IsReviewed)
            {
                TempData["SuccessMessage"] = "Ta wizyta została już oceniona.";
                return RedirectToAction("Index", "Notification");
            }

            
            ViewBag.DoctorName = appointment.Doctor?.LastName;
            ViewBag.ServiceName = appointment.Service?.Name;

            var review = new Review
            {
                AppointmentSlotId = appointmentId,
                UserId = userId
            };

            return View(review);
        }

        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AppointmentSlotId,Rating,Comment")] Review review)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            review.UserId = userId; 

            
            ModelState.Remove("UserId");
            ModelState.Remove("AppointmentSlot");
            

            if (ModelState.IsValid)
            {
                
                _context.Reviews.Add(review);

                
                var appointment = await _context.AppointmentSlots.FindAsync(review.AppointmentSlotId);
                if (appointment != null)
                {
                    appointment.IsReviewed = true;
                }

                
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.AppointmentSlotId == review.AppointmentSlotId && n.Type == "ProsbaOOpinie");

                if (notification != null)
                {
                    notification.IsRead = true;
                    notification.Message += " (Dziękujemy za opinię!)";
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Dziękujemy za pozostawienie opinii!";
                return RedirectToAction("Index", "PatientPanel");
            }

            
            return View(review);
        }
    }
}