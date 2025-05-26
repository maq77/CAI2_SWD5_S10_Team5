using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;
using Microsoft.Extensions.Options;
using TechXpress.Data.Model;

namespace TechXpress.Services
{
    public class EmailServer : IEmailServer
    {
        private readonly EmailSettings _settings;
        public EmailServer(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            email.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 30000
            };

            try
            {
                await client.SendMailAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                throw;
            }
        }

        public async Task SendVerificationEmailAsync(string to, string token)
        {
            var encodedToken = WebUtility.UrlEncode(token);
            Console.WriteLine($"[DEBUG] Raw token from URL: {token}");
            Console.WriteLine($"[DEBUG] Token length: {token?.Length}");
            Console.WriteLine($"[DEBUG] encoded token from URL: {encodedToken}");
            Console.WriteLine($"[DEBUG] encoded Token length: {encodedToken?.Length}");
            var verificationLink = $"{_settings.ClientAppUrl}/Account/ConfirmEmail?email={to}&token={encodedToken}";
            var subject = "Confirm your account registration";
            var message = $@"
                <p>Thank you for registering with TechXpress!</p>
                <p>Please confirm your email by clicking the link below:</p>
                <p><a href='{verificationLink}'>Confirm Email</a></p>
                <p>If you didn’t request this, please ignore this email.</p>";

            var email = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            email.To.Add(to);

            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 30000
            };

            try
            {
                await client.SendMailAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP error: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                throw;
            }
        }
    }
}
