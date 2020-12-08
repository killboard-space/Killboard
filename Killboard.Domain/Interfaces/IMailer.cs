using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Killboard.Domain.Interfaces
{
    public interface IMailer
    {
        Task SendEmailAsync(List<string> emails, string subject, string message);
    }
}
