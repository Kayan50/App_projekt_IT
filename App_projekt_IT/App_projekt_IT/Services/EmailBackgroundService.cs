using System.Net;
using System.Net.Mail;

namespace App_projekt_IT.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IEmailSenderQueue _queue;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(IEmailSenderQueue queue, IConfiguration configuration, ILogger<EmailBackgroundService> logger)
        {
            _queue = queue;
            _configuration = configuration;
            _logger = logger;
        }

        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serwis mailowy w tle został uruchomiony.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
                    var message = await _queue.DequeueEmailAsync(stoppingToken);

                    
                    await SendEmailAsync(message);
                }
                catch (OperationCanceledException)
                {
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wysyłania maila w tle.");
                }
            }
        }

        private async Task SendEmailAsync(EmailMessage message)
        {
            var host = _configuration["SmtpSettings:Host"];
            var port = int.Parse(_configuration["SmtpSettings:Port"]);
            var username = _configuration["SmtpSettings:Username"];
            var password = _configuration["SmtpSettings:Password"];
            var senderEmail = _configuration["SmtpSettings:SenderEmail"];
            var senderName = _configuration["SmtpSettings:SenderName"];

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true 
            };
            mailMessage.To.Add(message.ToEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"Pomyślnie wysłano maila do: {message.ToEmail}");
        }
    }
}