using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Text.RegularExpressions;
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
            
            var match = Regex.Match(htmlMessage, @"href=[""']([^""']+)[""']");
            string callbackUrl = match.Success ? match.Groups[1].Value : "#";

            
            string title = "Powiadomienie systemowe";
            string intro = "Otrzymaliśmy zgłoszenie powiązane z Twoim kontem w systemie pacjenta.";
            string buttonText = "Przejdź do strony";
            string outro = "Jeśli ta akcja nie była inicjowana przez Ciebie, możesz bezpiecznie zignorować tę wiadomość.";
            string cleanSubject = subject;

            
            switch (subject)
            {
                case "Confirm your email":
                    cleanSubject = "Klinika IT - Potwierdzenie adresu e-mail";
                    title = "Potwierdź swój adres e-mail";
                    intro = "Otrzymaliśmy prośbę o ponowne przesłanie linku aktywacyjnego dla Twojego konta w Klinice IT.";
                    buttonText = "Potwierdź adres e-mail";
                    outro = "Jeśli nie rejestrowałeś się w naszym systemie, po prostu zignoruj tę wiadomość.";
                    break;

                case "Reset Password":
                    cleanSubject = "Klinika IT - Resetowanie hasła do konta";
                    title = "Resetowanie hasła";
                    intro = "Otrzymaliśmy prośbę o zresetowanie hasła do Twojego konta w Klinice IT. Kliknij w poniższy przycisk, aby ustawić nowe hasło.";
                    buttonText = "Zresetuj hasło";
                    outro = "Jeśli nie prosiłeś o resetowanie hasła, Twoje konto jest bezpieczne i możesz zignorować tę wiadomość.";
                    break;

                case "Change Email":
                    cleanSubject = "Klinika IT - Potwierdzenie zmiany adresu e-mail";
                    title = "Potwierdź zmianę adresu e-mail";
                    intro = "Otrzymaliśmy prośbę o zmianę adresu e-mail dla Twojego konta w Klinice IT. Aby dokończyć ten proces, musimy zweryfikować Twoją nową skrzynkę pocztową.";
                    buttonText = "Potwierdź nowy adres e-mail";
                    outro = "Jeśli to nie Ty zlecałeś tę zmianę, zignoruj tę wiadomość lub skontaktuj się z naszą administracją.";
                    break;
            }

            
            var beautifulBody = $@"
                <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto; padding: 20px;'>
                    <h2 style='color: #2563eb;'>{title}</h2>
                    <p>{intro}</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{callbackUrl}' style='background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>{buttonText}</a>
                    </div>
                    <p>{outro}</p>
                    <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                    <p style='font-size: 0.8em; color: #94a2b8;'>Pozdrawiamy,<br/>Zespół Kliniki IT</p>
                </div>";

            var emailMessage = new EmailMessage
            {
                ToEmail = email,
                Subject = cleanSubject,
                Body = beautifulBody
            };

            await _emailQueue.QueueEmailAsync(emailMessage);
        }
    }
}