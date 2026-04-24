using ASCWeb1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASCWeb1.Controllers
{
    public class TestEmailController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public TestEmailController(IEmailSender emailSender, IConfiguration configuration)
        {
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.SMTPAccount = _configuration["ApplicationSettings:SMTPAccount"];
            ViewBag.SMTPServer = _configuration["ApplicationSettings:SMTPServer"];
            ViewBag.SMTPPort = _configuration["ApplicationSettings:SMTPPort"];
            ViewBag.PasswordLength = _configuration["ApplicationSettings:SMTPPassword"]?.Length ?? 0;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Send(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Json(new { success = false, message = "Email không được để trống" });
            }

            try
            {
                await _emailSender.SendEmailAsync(
                    email,
                    "Test Email - Automobile Service Center",
                    @"<html>
                        <body style='font-family: Arial, sans-serif; padding: 20px;'>
                            <h2 style='color: #37474f;'>Test Email</h2>
                            <p>Đây là email test từ Automobile Service Center.</p>
                            <p>Nếu bạn nhận được email này, cấu hình SMTP đã hoạt động!</p>
                            <hr>
                            <p style='color: #999; font-size: 12px;'>Email tự động - vui lòng không trả lời</p>
                        </body>
                    </html>");

                return Json(new { success = true, message = $"Email đã gửi thành công đến {email}. Vui lòng kiểm tra hộp thư (và spam)." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
