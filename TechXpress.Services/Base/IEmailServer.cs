using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.Base
{
    public interface IEmailServer
    {
        /// Sends an email with the specified subject and body to the given recipient.
        Task SendEmailAsync(string toEmail, string subject, string message);
        /// Sends a verification email to the specified recipient.
        Task SendVerificationEmailAsync(string to, string token);
    }
}
