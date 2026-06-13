using App_projekt_IT.Data;
using App_projekt_IT.Models;
using App_projekt_IT.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace App_projekt_IT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSenderQueue _emailQueue; // Dodana kolejka

        public AdminSchedulesController(ApplicationDbContext context, IEmailSenderQueue emailQueue)
        {
            _context = context;
            _emailQueue = emailQueue;
        }

        // GET: Wyświetla terminy
        public async Task<IActionResult> Index(int? clinicId, int? doctorId, DateTime? searchDate)
        {
            ViewBag.ClinicId = new SelectList(_context.Clinics, "Id", "Name", clinicId);

            var doctors = _context.Doctors.Select(d => new
            {
                Id = d.Id,
                FullName = d.FirstName + " " + d.LastName
            }).ToList();
            ViewBag.DoctorId = new SelectList(doctors, "Id", "FullName", doctorId);

            ViewBag.CurrentDate = searchDate?.ToString("yyyy-MM-dd");

            bool searchPerformed = clinicId.HasValue || doctorId.HasValue || searchDate.HasValue;
            ViewBag.SearchPerformed = searchPerformed;

            if (!searchPerformed)
            {
                return View(new List<AppointmentSlot>());
            }

            var query = _context.AppointmentSlots
                .Include(s => s.Doctor)
                .ThenInclude(d => d.Clinic)
                .Include(s => s.User)
                .AsQueryable();

            if (clinicId.HasValue)
            {
                query = query.Where(s => s.Doctor.ClinicId == clinicId.Value);
            }
            if (doctorId.HasValue)
            {
                query = query.Where(s => s.DoctorId == doctorId.Value);
            }
            if (searchDate.HasValue)
            {
                query = query.Where(s => s.StartTime.Date == searchDate.Value.Date);
            }

            var slots = await query.OrderBy(s => s.StartTime).ToListAsync();
            return View(slots);
        }

        // --- DLA POJEDYNCZYCH TERMINÓW ---

        [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CancelAppointment(int id, int? clinicId, int? doctorId, DateTime? searchDate)
{
    var slot = await _context.AppointmentSlots
        .Include(s => s.User)
        .Include(s => s.Doctor)
        .FirstOrDefaultAsync(s => s.Id == id);

    if (slot != null && slot.UserId != null)
    {
        string doctorFullName = $"{slot.Doctor.Title} {slot.Doctor.FirstName} {slot.Doctor.LastName}";
        CreateSystemNotification(slot.UserId, doctorFullName, slot.StartTime);
        await SendCancellationEmailAsync(slot.User.Email, slot.User.FirstName, doctorFullName, slot.StartTime);

        slot.UserId = null; 
        _context.Update(slot);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Wizyta odwołana. Powiadomienia zostały wysłane do pacjenta.";
    }
    
    // Zmieniony powrót: zachowujemy filtry
    return RedirectToAction(nameof(Index), new { clinicId, doctorId, searchDate = searchDate?.ToString("yyyy-MM-dd") });
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSlot(int id, int? clinicId, int? doctorId, DateTime? searchDate)
        {
            var slot = await _context.AppointmentSlots
                .Include(s => s.User)
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (slot != null)
            {
                if (slot.UserId != null)
                {
                    string doctorFullName = $"{slot.Doctor.Title} {slot.Doctor.FirstName} {slot.Doctor.LastName}";
                    CreateSystemNotification(slot.UserId, doctorFullName, slot.StartTime);
                    await SendCancellationEmailAsync(slot.User.Email, slot.User.FirstName, doctorFullName, slot.StartTime);
                }

                var relatedNotifications = await _context.Notifications.Where(n => n.AppointmentSlotId == id).ToListAsync();
                if (relatedNotifications.Any()) _context.Notifications.RemoveRange(relatedNotifications);

                _context.AppointmentSlots.Remove(slot);
                await _context.SaveChangesAsync();
        
                TempData["SuccessMessage"] = "Termin usunięty z grafiku.";
            }
    
    // Zmieniony powrót: zachowujemy filtry
    return RedirectToAction(nameof(Index), new { clinicId, doctorId, searchDate = searchDate?.ToString("yyyy-MM-dd") });
}

        // --- MASOWE USUWANIE ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MassDelete(int? clinicId, int? doctorId, DateTime? searchDate)
        {
            if (!searchDate.HasValue) return RedirectToAction(nameof(Index));

            var query = _context.AppointmentSlots
                .Include(s => s.User)
                .Include(s => s.Doctor)
                .Where(s => s.StartTime.Date == searchDate.Value.Date);

            if (clinicId.HasValue) query = query.Where(s => s.Doctor.ClinicId == clinicId.Value);
            if (doctorId.HasValue) query = query.Where(s => s.DoctorId == doctorId.Value);

            var slotsToDelete = await query.ToListAsync();
            if (!slotsToDelete.Any()) return RedirectToAction(nameof(Index));

            var bookedSlots = slotsToDelete.Where(s => s.UserId != null).ToList();

            foreach (var slot in bookedSlots)
            {
                string doctorFullName = $"{slot.Doctor.Title} {slot.Doctor.FirstName} {slot.Doctor.LastName}";
                CreateSystemNotification(slot.UserId, doctorFullName, slot.StartTime);
                await SendCancellationEmailAsync(slot.User.Email, slot.User.FirstName, doctorFullName, slot.StartTime);
            }

            var slotIdsToDelete = slotsToDelete.Select(s => s.Id).ToList();
            var notificationsToDelete = await _context.Notifications
                .Where(n => n.AppointmentSlotId != null && slotIdsToDelete.Contains((int)n.AppointmentSlotId))
                .ToListAsync();

            if (notificationsToDelete.Any()) _context.Notifications.RemoveRange(notificationsToDelete);
            _context.AppointmentSlots.RemoveRange(slotsToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Usunięto {slotsToDelete.Count} terminów. Powiadomiono {bookedSlots.Count} pacjentów.";
            return RedirectToAction(nameof(Index));
        }
        // --- API DLA KASKADOWEJ LISTY LEKARZY ---
        [HttpGet]
        public async Task<JsonResult> GetDoctorsByClinic(int? clinicId)
        {
            var query = _context.Doctors.AsQueryable();

            if (clinicId.HasValue && clinicId.Value > 0)
            {
                query = query.Where(d => d.ClinicId == clinicId.Value);
            }

            var doctors = await query.Select(d => new {
                id = d.Id,
                fullName = d.FirstName + " " + d.LastName
            }).ToListAsync();

            return Json(doctors);
        }

        // --- FUNKCJE POMOCNICZE DO POWIADOMIEŃ ---

        private async Task SendCancellationEmailAsync(string email, string patientName, string doctorName, DateTime startTime)
        {
            var title = "Odwołanie wizyty";
            var intro = $"Witaj {patientName},";
            var message = $"Z przykrością informujemy, że Twoja wizyta u specjalisty <strong>{doctorName}</strong> zaplanowana na <strong>{startTime.ToString("dd.MM.yyyy HH:mm")}</strong> została odwołana przez administrację kliniki.";
            var outro = "Przepraszamy za niedogodności. Prosimy o zalogowanie się do systemu w celu rezerwacji nowego terminu.";

            var beautifulBody = $@"
                <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; padding: 20px;'>
                    <h2 style='color: #e11d48;'>{title}</h2>
                    <p>{intro}</p>
                    <p>{message}</p>
                    <p>{outro}</p>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                    <p style='font-size: 0.8em; color: #94a2b8;'>Pozdrawiamy,<br/>Zespół Kliniki-Med</p>
                </div>";

            var emailMessage = new EmailMessage
            {
                ToEmail = email,
                Subject = "Klinika-Med - Odwołanie wizyty",
                Body = beautifulBody
            };

            await _emailQueue.QueueEmailAsync(emailMessage);
        }

        private void CreateSystemNotification(string userId, string doctorName, DateTime startTime)
        {
            var notification = new Models.Notification
            {
                UserId = userId,
                Message = $"Twoja wizyta u specjalisty {doctorName} w dniu {startTime.ToString("dd.MM.yyyy HH:mm")} została odwołana.",
                CreatedAt = DateTime.Now,
                IsRead = false,
            };
            _context.Notifications.Add(notification);
        }
    }
}