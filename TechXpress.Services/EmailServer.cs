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
using Microsoft.AspNetCore.Http;

namespace TechXpress.Services
{
    public class EmailServer : IEmailServer
    {
        private  EmailSettings _settings;
        private readonly IDynamicSettingsService _dynamicSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailServer(IOptions<EmailSettings> options, IDynamicSettingsService dynamicSettings, IHttpContextAccessor httpContextAccessor)
        {
            _settings = options.Value;
            _dynamicSettings = dynamicSettings;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<string> GetBaseUrlAsync()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                var scheme = request.Scheme; // http / https
                var host = request.Host.Value; // domain and port (if there)
                return $"{scheme}://{host}";
            }
            _settings.ClientAppUrl = await _dynamicSettings.GetValueAsync("EmailSettings:ClientAppUrl");
            // Fallback to configuration if HTTP context not available
            return _settings.ClientAppUrl ?? "https://localhost";
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var email = CreateBaseEmail(toEmail, subject);

            // Add both HTML and plain text
            email.Body = message;
            email.IsBodyHtml = true;

            // Add plain text alternative
            var plainTextView = AlternateView.CreateAlternateViewFromString(
                StripHtml(message),
                Encoding.UTF8,
                "text/plain"
            );
            var htmlView = AlternateView.CreateAlternateViewFromString(
                message,
                Encoding.UTF8,
                "text/html"
            );

            email.AlternateViews.Add(plainTextView);
            email.AlternateViews.Add(htmlView);

            await SendEmailWithClient(email);
        }

        public async Task SendVerificationEmailAsync(string to, string token)
        {
            var baseUrl = GetBaseUrlAsync();
            var encodedToken = WebUtility.UrlEncode(token);
            Console.WriteLine($"[DEBUG] Using base URL: {baseUrl}");
            Console.WriteLine($"[DEBUG] Raw token: {token}");
            Console.WriteLine($"[DEBUG] Encoded token: {encodedToken}");

            var verificationLink = $"{baseUrl}/Account/ConfirmEmail?email={WebUtility.UrlEncode(to)}&token={encodedToken}";

            // More personalized subject
            var subject = "Welcome to TechXpress - Please verify your email";

            // Improved HTML with better structure
            var htmlMessage = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Verification</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; border: 1px solid #e9ecef;'>
        <h2 style='color: #007bff; margin-bottom: 20px;'>Welcome to TechXpress!</h2>
        
        <p style='margin-bottom: 20px;'>Thank you for creating your account with us. We're excited to have you on board!</p>
        
        <p style='margin-bottom: 25px;'>To complete your registration and secure your account, please verify your email address by clicking the button below:</p>
        
        <div style='text-align: center; margin: 30px 0;'>
            <a href='{verificationLink}' 
               style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                Verify My Email Address
            </a>
        </div>
        
        <p style='margin-bottom: 15px; font-size: 14px; color: #666;'>
            If the button doesn't work, you can copy and paste this link into your browser:
        </p>
        <p style='word-break: break-all; font-size: 12px; color: #007bff; margin-bottom: 25px;'>
            {verificationLink}
        </p>
        
        <hr style='border: none; border-top: 1px solid #e9ecef; margin: 25px 0;'>
        
        <p style='font-size: 12px; color: #666; margin-bottom: 10px;'>
            This verification link will expire in 24 hours for security reasons.
        </p>
        <p style='font-size: 12px; color: #666;'>
            If you didn't create an account with TechXpress, please ignore this email or contact our support team.
        </p>
        
        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #e9ecef; text-align: center;'>
            <p style='font-size: 12px; color: #666; margin: 0;'>
                Best regards,<br>
                The TechXpress Team<br>
                <a href='{baseUrl}' style='color: #007bff;'>{baseUrl}</a>
            </p>
        </div>
    </div>
</body>
</html>";

            // Plain text version
            var plainTextMessage = $@"
Welcome to TechXpress!

Thank you for creating your account with us. We're excited to have you on board!

To complete your registration and secure your account, please verify your email address by visiting this link:

{verificationLink}

This verification link will expire in 24 hours for security reasons.

If you didn't create an account with TechXpress, please ignore this email or contact our support team.

Best regards,
The TechXpress Team
{baseUrl}
";

            var email = CreateBaseEmail(to, subject);

            // Add both versions
            var plainTextView = AlternateView.CreateAlternateViewFromString(plainTextMessage, Encoding.UTF8, "text/plain");
            var htmlView = AlternateView.CreateAlternateViewFromString(htmlMessage, Encoding.UTF8, "text/html");

            email.AlternateViews.Add(plainTextView);
            email.AlternateViews.Add(htmlView);

            await SendEmailWithClient(email);
        }

        private MailMessage CreateBaseEmail(string toEmail, string subject)
        {
            _settings = _dynamicSettings.GetSectionAsync<EmailSettings>("EmailSettings").GetAwaiter().GetResult();
            var email = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                IsBodyHtml = true
            };

            email.To.Add(toEmail);
            var baseUrl = GetBaseUrlAsync();
            // Add  headers for deliverability
            email.Headers.Add("List-Unsubscribe", $"<{baseUrl}/unsubscribe>");
            email.Headers.Add("X-Mailer", "TechXpress-EmailService/1.0");
            email.Headers.Add("Message-ID", $"<{Guid.NewGuid()}@{GetDomainFromEmail(_settings.SenderEmail)}>");

            return email;
        }

        private async Task SendEmailWithClient(MailMessage email)
        {
            _settings = _dynamicSettings.GetSectionAsync<EmailSettings>("EmailSettings").GetAwaiter().GetResult();
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
                Console.WriteLine($"[INFO] Email sent successfully to {email.To.FirstOrDefault()?.Address}");
            }
            catch (SmtpException ex)
            {
                Console.WriteLine($"[ERROR] SMTP error: {ex.Message}");
                Console.WriteLine($"[ERROR] Status Code: {ex.StatusCode}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[ERROR] Inner: {ex.InnerException.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] General email error: {ex.Message}");
                throw;
            }
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            return System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ")
                .Replace("&nbsp;", " ")
                .Replace("&amp;", "&")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Trim();
        }

        private string GetDomainFromEmail(string email)
        {
            return email.Split('@').LastOrDefault() ?? "localhost";
        }
    }
}