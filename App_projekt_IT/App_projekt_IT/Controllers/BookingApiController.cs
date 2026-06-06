using App_projekt_IT.Data;
using App_projekt_IT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace App_projekt_IT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class BookingApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: 
        [HttpPost("{slotId}")]
        public async Task<IActionResult> BookAppointment(int slotId)
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Błąd autoryzacji sesji." });
            }

            
            var rowsAffected = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE AppointmentSlots SET IsBooked = 1, UserId = {userId} WHERE Id = {slotId} AND IsBooked = 0"
            );

            
            if (rowsAffected == 0)
            {
               
                return BadRequest(new { message = "Ten termin jest już niedostępny lub został zarezerwowany przez inną osobę." });
            }

      
            var appointment = await _context.AppointmentSlots.FindAsync(slotId);

            if (appointment != null)
            {
                
                var timeUntilAppointment = appointment.StartTime - DateTime.Now;

                
                bool isShortNotice = timeUntilAppointment.TotalDays <= 5;
                string extraMessage = "";

                if (isShortNotice)
                {
                    
                    appointment.IsConfirmed = true;
                    extraMessage = " (Wizyta została automatycznie potwierdzona ze względu na krótki czas do terminu).";
                }

                var notification = new Notification
                {
                    UserId = userId,
                    AppointmentSlotId = slotId,
                    Message = $"Pomyślnie zarezerwowano wizytę na dzień {appointment.StartTime:dd.MM.yyyy HH:mm}.{extraMessage}",
                    Type = "Info"
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            

            return Ok(new { message = "Wizyta została pomyślnie zarezerwowana." });
        }
    }
}