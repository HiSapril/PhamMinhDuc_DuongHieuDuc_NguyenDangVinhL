// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace ASCWeb1.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
        }
        
        public IActionResult OnGet()
        {
            // Không redirect về Login nữa, để Google callback có thể hoạt động
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            _logger.LogInformation("Redirecting to {Provider} for authentication. Callback URL: {CallbackUrl}", provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            
            _logger.LogInformation("External login callback received. ReturnUrl: {ReturnUrl}, RemoteError: {RemoteError}", returnUrl, remoteError);
            
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                _logger.LogError("Remote error from external provider: {RemoteError}", remoteError);
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                _logger.LogError("GetExternalLoginInfoAsync returned null");
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            _logger.LogInformation("External login info retrieved. Provider: {Provider}, Email: {Email}", 
                info.LoginProvider, 
                info.Principal.FindFirstValue(ClaimTypes.Email));
            
            // Log tất cả claims để debug
            _logger.LogInformation("Available claims:");
            foreach (var claim in info.Principal.Claims)
            {
                _logger.LogInformation("  {Type}: {Value}", claim.Type, claim.Value);
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                
                // Lưu Session cho user đã tồn tại
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    HttpContext.Session.SetString("CurrentUserEmail", email);
                    
                    // Cập nhật avatar từ Google nếu có
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        // Lấy picture URL từ Google claims
                        var pictureClaim = info.Principal.FindFirst("picture") 
                            ?? info.Principal.FindFirst("urn:google:picture")
                            ?? info.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri");
                        
                        _logger.LogInformation("Picture claim for existing user: {Picture}", pictureClaim?.Value ?? "NULL");
                        
                        if (pictureClaim != null && !string.IsNullOrEmpty(pictureClaim.Value))
                        {
                            var userClaims = await _userManager.GetClaimsAsync(user);
                            var existingPictureClaim = userClaims.FirstOrDefault(c => c.Type == "picture");
                            
                            if (existingPictureClaim != null)
                            {
                                await _userManager.ReplaceClaimAsync(user, existingPictureClaim, new Claim("picture", pictureClaim.Value));
                                _logger.LogInformation("Updated picture claim: {Picture}", pictureClaim.Value);
                            }
                            else
                            {
                                await _userManager.AddClaimAsync(user, new Claim("picture", pictureClaim.Value));
                                _logger.LogInformation("Added picture claim: {Picture}", pictureClaim.Value);
                            }
                            
                            // QUAN TRỌNG: Sign out và sign in lại để claims được cập nhật vào cookie
                            await _signInManager.SignOutAsync();
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            _logger.LogInformation("Refreshed sign-in with updated claims");
                        }
                    }
                }
                
                // Redirect đến Dashboard
                return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out");
                return RedirectToPage("./Lockout");
            }
            else
            {
                // Lấy email từ Google
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                
                if (string.IsNullOrEmpty(email))
                {
                    ErrorMessage = "Email information is not available from Google.";
                    _logger.LogError("Email claim is missing from external login info");
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                // Kiểm tra xem email đã tồn tại trong hệ thống chưa
                var existingUser = await _userManager.FindByEmailAsync(email);
                
                if (existingUser != null)
                {
                    // Trường hợp email đã được đăng ký (Admin hoặc Engineer)
                    ErrorMessage = $"Email '{email}' is already taken.";
                    _logger.LogWarning("Email {Email} already exists in the system", email);
                    
                    // Hiển thị form với thông báo lỗi
                    ReturnUrl = returnUrl;
                    ProviderDisplayName = info.ProviderDisplayName;
                    Input = new InputModel { Email = email };
                    return Page();
                }
                else
                {
                    // Trường hợp email chưa đăng ký -> Tự động tạo tài khoản User mới
                    _logger.LogInformation("Creating new user account for {Email}", email);
                    
                    var user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };
                    
                    var createResult = await _userManager.CreateAsync(user);
                    
                    if (createResult.Succeeded)
                    {
                        // Lấy avatar từ Google - thử nhiều claim types khác nhau
                        var pictureClaim = info.Principal.FindFirst("picture") 
                            ?? info.Principal.FindFirst("urn:google:picture")
                            ?? info.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri");
                        
                        _logger.LogInformation("Picture claim value: {Picture}", pictureClaim?.Value ?? "NULL");
                        
                        // Thêm claims
                        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, user.Email));
                        await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, user.Email));
                        await _userManager.AddClaimAsync(user, new Claim("IsActive", "true"));
                        
                        // Lưu avatar từ Google
                        if (pictureClaim != null && !string.IsNullOrEmpty(pictureClaim.Value))
                        {
                            await _userManager.AddClaimAsync(user, new Claim("picture", pictureClaim.Value));
                            _logger.LogInformation("Saved picture claim: {Picture}", pictureClaim.Value);
                        }
                        else
                        {
                            _logger.LogWarning("No picture claim found from Google");
                            // Log tất cả claims để debug
                            _logger.LogInformation("All available claims from Google:");
                            foreach (var claim in info.Principal.Claims)
                            {
                                _logger.LogInformation("  Claim Type: {Type}, Value: {Value}", claim.Type, claim.Value);
                            }
                        }
                        
                        // Thêm role User
                        var roleResult = await _userManager.AddToRoleAsync(user, Roles.User.ToString());
                        
                        if (roleResult.Succeeded)
                        {
                            // Liên kết external login với user
                            var addLoginResult = await _userManager.AddLoginAsync(user, info);
                            
                            if (addLoginResult.Succeeded)
                            {
                                // Lưu Session giống như đăng nhập bằng email/password
                                HttpContext.Session.SetString("CurrentUserEmail", user.Email);
                                
                                // Đăng nhập
                                await _signInManager.SignInAsync(user, isPersistent: false);
                                _logger.LogInformation("User {Email} created and logged in with {Provider}", email, info.LoginProvider);
                                
                                // Redirect đến Dashboard giống như đăng nhập thường
                                return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
                            }
                            else
                            {
                                _logger.LogError("Failed to add external login: {Errors}", string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                            }
                        }
                        else
                        {
                            _logger.LogError("Failed to add User role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to create user: {Errors}", string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    }
                    
                    // Nếu có lỗi, hiển thị thông báo
                    ErrorMessage = "Unable to create user account. Please try again.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            // Phương thức này không còn được sử dụng vì đã tự động tạo tài khoản trong OnGetCallbackAsync
            // Giữ lại để tránh lỗi nếu có form submit
            returnUrl = returnUrl ?? Url.Content("~/");
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }
    }

    public enum Roles
    {
        Admin,
        Engineer,
        User
    }
}
