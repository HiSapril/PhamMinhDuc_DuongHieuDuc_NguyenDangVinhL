using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Logging;

namespace ASCWeb1.Services
{
    public class AuthMessageSender : ASCWeb1.Services.IEmailSender, 
                                      Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, 
                                      ISmsSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthMessageSender> _logger;

        public AuthMessageSender(IConfiguration configuration, ILogger<AuthMessageSender> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                _logger.LogInformation("=== BẮT ĐẦU GỬI EMAIL ===");
                _logger.LogInformation($"To: {email}");
                _logger.LogInformation($"Subject: {subject}");
                
                var smtpServer = _configuration["ApplicationSettings:SMTPServer"];
                var smtpPort = _configuration["ApplicationSettings:SMTPPort"];
                var smtpAccount = _configuration["ApplicationSettings:SMTPAccount"];
                var smtpPassword = _configuration["ApplicationSettings:SMTPPassword"];

                _logger.LogInformation($"SMTP Server: {smtpServer}");
                _logger.LogInformation($"SMTP Port: {smtpPort}");
                _logger.LogInformation($"SMTP Account: {smtpAccount}");
                _logger.LogInformation($"SMTP Password Length: {smtpPassword?.Length ?? 0}");

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpAccount) || string.IsNullOrEmpty(smtpPassword))
                {
                    throw new Exception("SMTP configuration is missing in appsettings.json");
                }

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("Automobile Service Center", smtpAccount));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = message };

                _logger.LogInformation("Đang kết nối đến SMTP server...");

                using (var client = new SmtpClient())
                {
                    // Enable logging for debugging
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    await client.ConnectAsync(smtpServer, int.Parse(smtpPort ?? "587"), MailKit.Security.SecureSocketOptions.StartTls);
                    _logger.LogInformation("Đã kết nối SMTP server thành công");

                    await client.AuthenticateAsync(smtpAccount, smtpPassword);
                    _logger.LogInformation("Đã xác thực SMTP thành công");

                    await client.SendAsync(emailMessage);
                    _logger.LogInformation("Đã gửi email thành công");

                    await client.DisconnectAsync(true);
                    _logger.LogInformation("Đã ngắt kết nối SMTP");
                }

                _logger.LogInformation($"✓ EMAIL ĐÃ GỬI THÀNH CÔNG ĐẾN: {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"✗ LỖI KHI GỬI EMAIL: {ex.Message}");
                _logger.LogError($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner Exception: {ex.InnerException.Message}");
                }

                throw; // Throw lại exception để controller biết có lỗi
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            _logger.LogInformation($"Send SMS to {number}");
            _logger.LogInformation($"Message: {message}");
            return Task.CompletedTask;
        }
    }
}
