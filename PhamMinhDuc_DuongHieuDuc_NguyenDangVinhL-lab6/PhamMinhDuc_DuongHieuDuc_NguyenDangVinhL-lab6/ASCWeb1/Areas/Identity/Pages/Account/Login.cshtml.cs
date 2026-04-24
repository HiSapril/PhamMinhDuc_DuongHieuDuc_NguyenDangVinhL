using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
// Namespace cho Session và Utilities
using ASC.Utilities;
using Microsoft.AspNetCore.Http;

namespace ASCWeb1.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager,
                          UserManager<IdentityUser> userManager,
                          ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public required InputModel Input { get; set; }

        public required IList<AuthenticationScheme> ExternalLogins { get; set; }
        public required string ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email không được để trống")]
            [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ")]
            public required string Email { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [DataType(DataType.Password)]
            public required string Password { get; set; }

            [Display(Name = "Ghi nhớ đăng nhập?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Xóa cookie bên ngoài để đảm bảo quy trình đăng nhập sạch
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // 1. Tìm User trong Database
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {
                    // 2. Lấy chuỗi PasswordHash ĐANG CÓ trong Database
                    var hashInDb = user.PasswordHash ?? string.Empty;

                    // 3. TỰ HASH mật khẩu người dùng vừa nhập bằng bộ máy của hệ thống hiện tại
                    var hashInput = _userManager.PasswordHasher.HashPassword(user, Input.Password);


                    // 4. KIỂM TRA KẾT QUẢ SO SÁNH (Verify)
                    var verifyResult = _userManager.PasswordHasher.VerifyHashedPassword(user, hashInDb, Input.Password);


                    // ĐẶT BREAKPOINT TẠI ĐÂY ĐỂ KIỂM TRA CÁC BIẾN:
                    // - hashInDb: Chuỗi băm cũ trong DB
                    // - hashInput: Chuỗi băm mới tạo ra từ mật khẩu vừa nhập
                    // - verifyResult: Kết quả (Success = 1, Failed = 0)

                    if (verifyResult == PasswordVerificationResult.Failed)
                    {
                        _logger.LogWarning($"DEBUG: Hash khong khop! DB: {hashInDb} | InputHash: {hashInput}");
                    }
                }

                // 1. Thực hiện đăng nhập
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Đăng nhập thành công!");

                    // Lưu Session
                    if (user?.Email != null)
                        HttpContext.Session.SetString("CurrentUserEmail", user.Email);

                    return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
                }

                // NẾU THẤT BẠI: Kiểm tra chi tiết lỗi
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản bị khóa.");
                }
                else if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản chưa được phép đăng nhập (có thể chưa xác nhận email).");
                }
                else
                {
                    // Nếu vào đây là chắc chắn SAI MẬT KHẨU so với chuỗi Hash trong DB
                    _logger.LogWarning($"Đăng nhập thất bại cho: {Input.Email}. Có thể sai mật khẩu.");
                    ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
                }
            }

            return Page();
        }
    }
}