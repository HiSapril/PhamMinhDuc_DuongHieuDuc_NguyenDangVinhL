using ASCWeb1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace ASCWeb1.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpPost]
        public async Task<IActionResult> RefreshAvatar()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Lấy external login info
                var logins = await _userManager.GetLoginsAsync(user);
                var googleLogin = logins.FirstOrDefault(l => l.LoginProvider == "Google");

                if (googleLogin == null)
                {
                    return Json(new { success = false, message = "No Google account linked" });
                }

                // Kiểm tra xem có picture claim không
                var userClaims = await _userManager.GetClaimsAsync(user);
                var pictureClaim = userClaims.FirstOrDefault(c => c.Type == "picture");

                if (pictureClaim != null)
                {
                    // Refresh sign-in để cập nhật claims vào cookie
                    await _signInManager.RefreshSignInAsync(user);
                    return Json(new { success = true, message = "Avatar refreshed successfully", avatarUrl = pictureClaim.Value });
                }
                else
                {
                    return Json(new { success = false, message = "No avatar found in claims" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> InitiateResetPassword()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                Console.WriteLine($"[RESET PASSWORD] User: {user.Email}");

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code = code },
                    protocol: Request.Scheme);

                Console.WriteLine($"[RESET PASSWORD] Callback URL: {callbackUrl}");

                var emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                            <h2 style='color: #37474f;'>Reset Your Password</h2>
                            <p>Hello,</p>
                            <p>You have requested to reset your password for your Automobile Service Center account.</p>
                            <p>Please click the button below to reset your password:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                                   style='background-color: #546e7a; color: white; padding: 12px 30px; 
                                          text-decoration: none; border-radius: 4px; display: inline-block;'>
                                    Reset Password
                                </a>
                            </div>
                            <p>Or copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #1976d2;'>{callbackUrl}</p>
                            <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                                If you did not request this password reset, please ignore this email.
                            </p>
                            <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                            <p style='color: #999; font-size: 11px;'>
                                This is an automated email from Automobile Service Center. Please do not reply.
                            </p>
                        </div>
                    </body>
                    </html>";

                Console.WriteLine($"[RESET PASSWORD] Sending email to: {user.Email}");
                
                await _emailSender.SendEmailAsync(
                    user.Email!,
                    "Reset Your Password - Automobile Service Center",
                    emailBody);

                Console.WriteLine($"[RESET PASSWORD] Email sent successfully to: {user.Email}");

                return RedirectToAction("ResetPasswordEmailConfirmation");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RESET PASSWORD ERROR] {ex.Message}");
                Console.WriteLine($"[RESET PASSWORD ERROR] Stack: {ex.StackTrace}");
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        public IActionResult ResetPasswordEmailConfirmation()
        {
            return View();
        }
    }
}
