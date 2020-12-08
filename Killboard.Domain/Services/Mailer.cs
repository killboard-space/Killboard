using Killboard.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Killboard.Domain.Services
{
    public class Mailer : IMailer
    {
        private readonly string _sendGridKey;

        public Mailer(IConfiguration configuration)
        {
            _sendGridKey = configuration["Killboard:SendGridKey"];
        }

        public Task SendEmailAsync(List<string> emails, string subject, string message)
        {
            return Execute(subject, message, emails);
        }

        public Task Execute(string subject, string message, List<string> emails)
        {
            var client = new SendGridClient(_sendGridKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("noreply@killboard.space", "killboard.space"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };

            foreach (var email in emails)
            {
                msg.AddTo(new EmailAddress(email));
            }

            Task response = client.SendEmailAsync(msg);
            return response;
        }
    }
}
