using App_projekt_IT.Data;
using App_projekt_IT.Models;
using App_projekt_IT.Services; 
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
        
        private readonly IEmailSenderQueue _emailQueue;

        
        public BookingApiController(ApplicationDbContext context, IEmailSenderQueue emailQueue)
        {
            _context = context;
            _emailQueue = emailQueue;
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

            
            var appointment = await _context.AppointmentSlots
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == slotId);

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

                
                var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name;

                if (!string.IsNullOrEmpty(userEmail))
                {
                    var emailMsg = new EmailMessage
                    {
                        ToEmail = userEmail,
                        Subject = "Potwierdzenie rezerwacji wizyty",
                        
                        Body = $@"
                            <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto;'>
                                <h2 style='color: #2563eb;'>Potwierdzenie rezerwacji</h2>
                                <p>Witaj,</p>
                                <p>Twoja wizyta została pomyślnie zarezerwowana w naszym systemie. Poniżej znajdują się jej szczegóły:</p>
                                <div style='background-color: #f8fafc; padding: 15px; border-radius: 8px; margin-top: 15px;'>
                                    <p><strong>Data:</strong> {appointment.StartTime:dd.MM.yyyy}</p>
                                    <p><strong>Godzina:</strong> {appointment.StartTime:HH:mm}</p>
                                    <p><strong>Usługa:</strong> {appointment.Service.Name}</p>
                                    <p><strong>Lekarz:</strong> {appointment.Doctor.Title} {appointment.Doctor.FirstName} {appointment.Doctor.LastName}</p>
                                    <p><strong>Adres:</strong> {appointment.Doctor.Clinic.Address} </p>
                                    
                                </div>
                                <p style='margin-top: 20px; font-size: 0.9em; color: #64748b;'>{extraMessage}</p>
                                <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                                <p style='font-size: 0.8em; color: #94a2b8;'>Pozdrawiamy,<br/>Zespół Kliniki-Med</p>
                            </div>"
                    };

                    
                    await _emailQueue.QueueEmailAsync(emailMsg);
                }
                
            }

            return Ok(new { message = "Wizyta została pomyślnie zarezerwowana." });
        }
    }
}