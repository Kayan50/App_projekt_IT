using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using App_projekt_IT.Data;
using App_projekt_IT.Models;
using Microsoft.AspNetCore.Identity;

namespace App_projekt_IT.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IEmailSenderQueue _emailQueue;

        public AppointmentReminderService(IServiceScopeFactory scopeFactory, IEmailSenderQueue emailQueue)
        {
            _scopeFactory = scopeFactory;
            _emailQueue = emailQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                var now = DateTime.Now;

                // ==================================================================
                // 1. WYSYŁKA: PRZYPOMNIENIA O POTWIERDZENIU (5 DNI PRZED WIZYTĄ)
                // ==================================================================
                var targetDate5Days = now.AddDays(5);
                var appointmentsToRemind = await context.AppointmentSlots
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .Where(a => a.IsBooked && !a.IsConfirmed && a.StartTime.Date == targetDate5Days.Date)
                    .ToListAsync(stoppingToken);

                foreach (var appt in appointmentsToRemind)
                {
                    bool alreadySent = await context.Notifications
                        .AnyAsync(n => n.AppointmentSlotId == appt.Id && n.Type == "WymagaPotwierdzenia");

                    if (!alreadySent && appt.UserId != null)
                    {
                        var notification = new Notification
                        {
                            UserId = appt.UserId,
                            AppointmentSlotId = appt.Id,
                            Message = $"Przypomnienie: Zbliża się Twoja wizyta zaplanowana na {appt.StartTime:dd.MM.yyyy HH:mm}. Prosimy o pilne potwierdzenie obecności (najpóźniej na 24h przed wizytą).",
                            Type = "WymagaPotwierdzenia"
                        };
                        context.Notifications.Add(notification);

                        var user = await userManager.FindByIdAsync(appt.UserId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            await _emailQueue.QueueEmailAsync(new EmailMessage
                            {
                                ToEmail = user.Email,
                                Subject = "Klinika IT - Ważne: Potwierdź swoją wizytę",
                                Body = $@"
                                    <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; line-height: 1.6;'>
                                        <h2 style='color: #d97706;'>Wymagane potwierdzenie wizyty</h2>
                                        <p>Witaj {user.FirstName},</p>
                                        <p>Przypominamy, że zbliża się termin Twojej zaplanowanej wizyty w naszej klinice. Aby utrzymać rezerwację, wymagane jest potwierdzenie obecności:</p>
                                        
                                        <div style='background-color: #f8fafc; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #d97706;'>
                                            <p style='margin: 4px 0;'><strong>Lekarz:</strong> {appt.Doctor.Title} {appt.Doctor.FirstName} {appt.Doctor.LastName}</p>
                                            <p style='margin: 4px 0;'><strong>Usługa:</strong> {appt.Service.Name}</p>
                                            <p style='margin: 4px 0;'><strong>Termin:</strong> {appt.StartTime:dd.MM.yyyy} r. o godz. {appt.StartTime:HH:mm}</p>
                                        </div>

                                        <p>Prosimy o pilne zalogowanie się do swojego panelu pacjenta i zatwierdzenie obecności. W przypadku braku potwierdzenia na 24 godziny przed wizytą, slot zostanie automatycznie zwolniony dla innych pacjentów.</p>
                                        <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                                        <p style='font-size: 0.8em; color: #94a2b8;'>Pozdrawiamy,<br/>Zespół Kliniki-Med</p>
                                    </div>"
                            });
                        }
                    }
                }

                // ==================================================================
                // 2. WYSYŁKA: AUTOMATYCZNE ODWOŁANIE (24H PRZED WIZYTĄ)
                // ==================================================================
                var targetDate24Hours = now.AddHours(24);
                var appointmentsToCancel = await context.AppointmentSlots
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .Where(a => a.IsBooked && !a.IsConfirmed && a.StartTime > now && a.StartTime <= targetDate24Hours)
                    .ToListAsync(stoppingToken);

                foreach (var appt in appointmentsToCancel)
                {
                    if (appt.UserId != null)
                    {
                        var notification = new Notification
                        {
                            UserId = appt.UserId,
                            AppointmentSlotId = null,
                            Message = $"Twoja wizyta w dniu {appt.StartTime:dd.MM.yyyy HH:mm} została automatycznie odwołana z powodu braku potwierdzenia obecności w wymaganym czasie.",
                            Type = "Anulowano"
                        };
                        context.Notifications.Add(notification);

                        var user = await userManager.FindByIdAsync(appt.UserId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            await _emailQueue.QueueEmailAsync(new EmailMessage
                            {
                                ToEmail = user.Email,
                                Subject = "Klinika IT - Anulowanie rezerwacji wizyty",
                                Body = $@"
                                    <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; line-height: 1.6;'>
                                        <h2 style='color: #dc2626;'>Wizyta została anulowana</h2>
                                        <p>Witaj {user.FirstName},</p>
                                        <p>Informujemy, że Twoja rezerwacja na poniższy termin musiała zostać automatycznie odwołana:</p>
                                        
                                        <div style='background-color: #f8fafc; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #dc2626;'>
                                            <p style='margin: 4px 0;'><strong>Lekarz:</strong> {appt.Doctor.Title} {appt.Doctor.FirstName} {appt.Doctor.LastName}</p>
                                            <p style='margin: 4px 0;'><strong>Usługa:</strong> {appt.Service.Name}</p>
                                            <p style='margin: 4px 0;'><strong>Niedoszły termin:</strong> {appt.StartTime:dd.MM.yyyy} o godz. {appt.StartTime:HH:mm}</p>
                                        </div>

                                        <p>Powodem anulowania był brak potwierdzenia obecności z Twojej strony w wymaganym czasie regulaminowym (minimum 24h przed wizytą). Jeśli wizyta jest nadal aktualna, prosimy o ponowne wyszukanie i zarezerwowanie wolnego terminu w systemie.</p>
                                        <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                                        <p style='font-size: 0.8em; color: #94a2b8;'>Pozdrawiamy,<br/>Zespół Kliniki-Med</p>
                                    </div>"
                            });
                        }
                    }

                    appt.IsBooked = false;
                    appt.UserId = null;
                    appt.IsConfirmed = false;
                }

                // ==================================================================
                // 3. WYSYŁKA: PROŚBA O OPINIĘ (PO ODBYTEJ WIZYCIE)
                // ==================================================================
                var timeForReview = now.AddHours(-2);
                var appointmentsToReview = await context.AppointmentSlots
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .Where(a => a.IsBooked && a.IsConfirmed && !a.IsReviewed && a.StartTime <= timeForReview)
                    .ToListAsync(stoppingToken);

                foreach (var appt in appointmentsToReview)
                {
                    bool alreadySent = await context.Notifications
                        .AnyAsync(n => n.AppointmentSlotId == appt.Id && n.Type == "ProsbaOOpinie", stoppingToken);

                    if (!alreadySent && appt.UserId != null)
                    {
                        var notification = new Notification
                        {
                            UserId = appt.UserId,
                            AppointmentSlotId = appt.Id,
                            Message = $"Twoja wizyta zaplanowana na {appt.StartTime:dd.MM.yyyy HH:mm} dobiegła końca. Przejdź do zakładki 'Moje wizyty', aby ocenić usługę i lekarza.",
                            Type = "ProsbaOOpinie"
                        };
                        context.Notifications.Add(notification);

                        var user = await userManager.FindByIdAsync(appt.UserId);
                        if (user != null && !string.IsNullOrEmpty(user.Email))
                        {
                            await _emailQueue.QueueEmailAsync(new EmailMessage
                            {
                                ToEmail = user.Email,
                                Subject = "Klinika IT - Dziękujemy za wizytę! Oceń nas",
                                Body = $@"
                                    <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; line-height: 1.6;'>
                                        <h2 style='color: #2563eb;'>Dziękujemy za zaufanie!</h2>
                                        <p>Witaj {user.FirstName},</p>
                                        <p>Mamy nadzieję, że Twoja dzisiejsza wizyta przebiegła pomyślnie i czujesz się dobrze zaopiekowany/a. Dbamy o najwyższą jakość usług w Klinice IT, dlatego szczegóły Twojej odbytej wizyty to:</p>
                                        
                                        <div style='background-color: #f8fafc; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #2563eb;'>
                                            <p style='margin: 4px 0;'><strong>Lekarz:</strong> {appt.Doctor.Title} {appt.Doctor.FirstName} {appt.Doctor.LastName}</p>
                                            <p style='margin: 4px 0;'><strong>Usługa:</strong> {appt.Service.Name}</p>
                                            <p style='margin: 4px 0;'><strong>Data wizyty:</strong> {appt.StartTime:dd.MM.yyyy}</p>
                                        </div>

                                        <p>Twoja opinia jest dla nas i innych pacjentów bezcenna. Będziemy wdzięczni, jeśli poświęcisz minutę na ocenę lekarza oraz wykonanej usługi. Możesz to zrobić w panelu pacjenta w sekcji <strong>'Moje wizyty'</strong>.</p>
                                        <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                                        <p style='font-size: 0.8em; color: #94a2b8;'>Pozdrawiamy,<br/>Zespół Kliniki-Med</p>
                                    </div>"
                            });
                        }
                    }
                }

                if (appointmentsToRemind.Any() || appointmentsToCancel.Any() || appointmentsToReview.Any())
                {
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }
}