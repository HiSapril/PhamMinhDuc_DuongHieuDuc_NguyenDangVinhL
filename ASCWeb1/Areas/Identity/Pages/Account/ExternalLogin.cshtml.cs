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
            // Redirect về Login vì không cần hiển thị form này nữa
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
                        var pictureClaim = info.Principal.FindFirst("picture") 
                            ?? info.Principal.FindFirst("urn:google:picture")
                            ?? info.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri");
                        
                        if (pictureClaim != null && !string.IsNullOrEmpty(pictureClaim.Value))
                        {
                            var userClaims = await _userManager.GetClaimsAsync(user);
                            var existingPictureClaim = userClaims.FirstOrDefault(c => c.Type == "picture");
                            
                            if (existingPictureClaim != null)
                            {
                                await _userManager.ReplaceClaimAsync(user, existingPictureClaim, new Claim("picture", pictureClaim.Value));
                            }
                            else
                            {
                                await _userManager.AddClaimAsync(user, new Claim("picture", pictureClaim.Value));
                            }
                            
                            await _signInManager.RefreshSignInAsync(user);
                        }
                    }
                }
                
                return LocalRedirect(returnUrl);
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
                    // Email đã tồn tại -> Liên kết external login và đăng nhập
                    _logger.LogInformation("Email {Email} already exists. Linking external login.", email);
                    
                    var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
                    
                    if (addLoginResult.Succeeded)
                    {
                        HttpContext.Session.SetString("CurrentUserEmail", existingUser.Email);
                        
                        // Cập nhật avatar
                        var pictureClaim = info.Principal.FindFirst("picture") 
                            ?? info.Principal.FindFirst("urn:google:picture")
                            ?? info.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri");
                        
                        if (pictureClaim != null && !string.IsNullOrEmpty(pictureClaim.Value))
                        {
                            var userClaims = await _userManager.GetClaimsAsync(existingUser);
                            var existingPictureClaim = userClaims.FirstOrDefault(c => c.Type == "picture");
                            
                            if (existingPictureClaim != null)
                            {
                                await _userManager.ReplaceClaimAsync(existingUser, existingPictureClaim, new Claim("picture", pictureClaim.Value));
                            }
                            else
                            {
                                await _userManager.AddClaimAsync(existingUser, new Claim("picture", pictureClaim.Value));
                            }
                        }
                        
                        await _signInManager.SignInAsync(existingUser, isPersistent: false);
                        _logger.LogInformation("Linked and logged in user {Email} with {Provider}", email, info.LoginProvider);
                        
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        ErrorMessage = $"Cannot link your Google account. {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}";
                        _logger.LogError("Failed to link external login: {Errors}", string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
                        return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                    }
                }
                else
                {
                    // Email chưa tồn tại -> Chuyển sang trang đăng ký với thông tin từ Google
                    ReturnUrl = returnUrl;
                    ProviderDisplayName = info.ProviderDisplayName;
                    
                    if (Input == null)
                    {
                        Input = new InputModel();
                    }
                    
                    Input.Email = email;
                    
                    // Lưu thông tin Google vào TempData để dùng khi submit form
                    TempData["GoogleEmail"] = email;
                    TempData["GoogleProvider"] = info.LoginProvider;
                    TempData["GoogleProviderKey"] = info.ProviderKey;
                    
                    var pictureClaim = info.Principal.FindFirst("picture") 
                        ?? info.Principal.FindFirst("urn:google:picture")
                        ?? info.Principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/uri");
                    
                    if (pictureClaim != null)
                    {
                        TempData["GooglePicture"] = pictureClaim.Value;
                    }
                    
                    _logger.LogInformation("New user {Email}, showing registration form", email);
                    
                    // Hiển thị form đăng ký
                    return Page();
                }
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            
            // Lấy thông tin Google từ TempData
            var googleEmail = TempData["GoogleEmail"] as string;
            var googleProvider = TempData["GoogleProvider"] as string;
            var googleProviderKey = TempData["GoogleProviderKey"] as string;
            var googlePicture = TempData["GooglePicture"] as string;
            
            if (string.IsNullOrEmpty(googleEmail) || string.IsNullOrEmpty(googleProvider) || string.IsNullOrEmpty(googleProviderKey))
            {
                ErrorMessage = "Session expired. Please try logging in again.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            
            if (ModelState.IsValid)
            {
                // Tạo user mới
                var user = new IdentityUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    EmailConfirmed = true
                };
                
                var createResult = await _userManager.CreateAsync(user);
                
                if (createResult.Succeeded)
                {
                    // Thêm claims
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Name, user.Email));
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Email, user.Email));
                    await _userManager.AddClaimAsync(user, new Claim("IsActive", "true"));
                    
                    // Lưu avatar từ Google
                    if (!string.IsNullOrEmpty(googlePicture))
                    {
                        await _userManager.AddClaimAsync(user, new Claim("picture", googlePicture));
                    }
                    
                    // Thêm role User
                    var roleResult = await _userManager.AddToRoleAsync(user, Roles.User.ToString());
                    
                    if (roleResult.Succeeded)
                    {
                        // Liên kết external login
                        var loginInfo = new UserLoginInfo(googleProvider, googleProviderKey, googleProvider);
                        var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                        
                        if (addLoginResult.Succeeded)
                        {
                            HttpContext.Session.SetString("CurrentUserEmail", user.Email);
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            _logger.LogInformation("User {Email} registered and logged in with {Provider}", user.Email, googleProvider);
                            
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            foreach (var error in addLoginResult.Errors)
                            {
                                ModelState.AddModelError(string.Empty, error.Description);
                            }
                        }
                    }
                    else
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            
            // Nếu có lỗi, giữ lại TempData và hiển thị lại form
            TempData.Keep();
            ProviderDisplayName = googleProvider;
            ReturnUrl = returnUrl;
            return Page();
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
