using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using App_projekt_IT.Data;
using App_projekt_IT.Models;

namespace App_projekt_IT.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        
        public AppointmentReminderService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var now = DateTime.Now;

               
                var targetDate5Days = now.AddDays(5);

                var appointmentsToRemind = await context.AppointmentSlots
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
                    }
                }

               
                var targetDate24Hours = now.AddHours(24);

                var appointmentsToCancel = await context.AppointmentSlots
                    .Where(a => a.IsBooked && !a.IsConfirmed
                             && a.StartTime > now 
                             && a.StartTime <= targetDate24Hours) 
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
                    }

                    
                    appt.IsBooked = false;
                    appt.UserId = null;
                    appt.IsConfirmed = false;
                }

                
                var timeForReview = now.AddHours(-2);

                var appointmentsToReview = await context.AppointmentSlots
                    .Where(a => a.IsBooked
                             && a.IsConfirmed 
                             && !a.IsReviewed 
                             && a.StartTime <= timeForReview)
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