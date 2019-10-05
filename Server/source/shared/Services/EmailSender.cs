using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Jwtauth.Config;

namespace Jwtauth.Services
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<SendGridSettings> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public SendGridSettings Options { get; }  

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.Secret, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("robot@Jwtauth", "Jwtauth Robot"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}