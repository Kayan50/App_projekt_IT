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
                                Subject = "Klinika IT: Ważne - Potwierdź swoją wizytę",
                                Body = $@"<h2 style='color:#f59e0b;'>Wymagane potwierdzenie wizyty</h2>
                                          <p>Witaj {user.FirstName},</p>
                                          <p>Przypominamy, że na dzień <strong>{appt.StartTime:dd.MM.yyyy HH:mm}</strong> masz zaplanowaną wizytę (Usługa: {appt.Service.Name}, Lekarz: {appt.Doctor.LastName}).</p>
                                          <p>Prosimy o pilne zalogowanie się do panelu pacjenta i potwierdzenie swojej obecności. W przeciwnym razie wizyta zostanie automatycznie anulowana na 24 godziny przed terminem.</p>"
                            });
                        }
                    }
                }

                
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
                                Subject = "Klinika IT: Wizyta została anulowana",
                                Body = $@"<h2 style='color:#ef4444;'>Wizyta anulowana</h2>
                                          <p>Witaj {user.FirstName},</p>
                                          <p>Informujemy, że Twoja wizyta zaplanowana na <strong>{appt.StartTime:dd.MM.yyyy HH:mm}</strong> (Usługa: {appt.Service.Name}, Lekarz: {appt.Doctor.LastName}) została automatycznie anulowana.</p>
                                          <p>Powodem odwołania był brak potwierdzenia obecności z Twojej strony w wymaganym czasie (minimum 24h przed wizytą).</p>"
                            });
                        }
                    }

                    appt.IsBooked = false;
                    appt.UserId = null;
                    appt.IsConfirmed = false;
                }

                
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
                                Subject = "Klinika IT: Jak oceniasz swoją wizytę?",
                                Body = $@"<h2 style='color:#3b82f6;'>Dziękujemy za wizytę!</h2>
                                          <p>Witaj {user.FirstName},</p>
                                          <p>Mamy nadzieję, że Twoja dzisiejsza wizyta (Usługa: {appt.Service.Name}) u lekarza {appt.Doctor.LastName} przebiegła pomyślnie.</p>
                                          <p>Twoja opinia jest dla nas bardzo ważna! Zaloguj się do panelu pacjenta i przejdź do zakładki 'Moje wizyty', aby zostawić ocenę i pomóc innym pacjentom w wyborze specjalisty.</p>"
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