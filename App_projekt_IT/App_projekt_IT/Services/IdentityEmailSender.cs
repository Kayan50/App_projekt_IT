using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace App_projekt_IT.Services
{
    public class IdentityEmailSender : IEmailSender
    {
        private readonly IEmailSenderQueue _emailQueue;

        public IdentityEmailSender(IEmailSenderQueue emailQueue)
        {
            _emailQueue = emailQueue;
        }

        
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailMessage = new EmailMessage
            {
                ToEmail = email,
                Subject = subject,
                Body = htmlMessage 
            };

            
            await _emailQueue.QueueEmailAsync(emailMessage);
        }
    }
}