using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using App_projekt_IT.Data;
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

            // 3. Weryfikacja sukcesu
            if (rowsAffected == 0)
            {
               
                return BadRequest(new { message = "Ten termin jest już niedostępny lub został zarezerwowany przez inną osobę." });
            }

            return Ok(new { message = "Wizyta została pomyślnie zarezerwowana." });
        }
    }
}