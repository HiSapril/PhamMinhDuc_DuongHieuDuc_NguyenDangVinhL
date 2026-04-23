using ASCWeb1.Areas.Accounts.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ASC.Utilities;
using ASC.Model;

namespace ASCWeb1.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IEmailSender emailSender, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var customers = await _userManager.GetUsersInRoleAsync(Roles.User.ToString());
            // Hold all service engineers in session
            HttpContext.Session.SetSession("Customers", customers);
            return View(new CustomerViewModel
            {
                Customers = customers?.ToList() ?? new List<IdentityUser>(),
                Registration = new CustomerRegistrationViewModel() { IsEdit = false }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customers(CustomerViewModel customer)
        {
            customer.Customers = HttpContext.Session.GetSession<List<IdentityUser>>("Customers");
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            if (customer.Registration.IsEdit)
            {
                // update user
                // update claims IsActive
                var user = await _userManager.FindByEmailAsync(customer.Registration.Email);
                if (user != null)
                {
                    var identity = await _userManager.GetClaimsAsync(user);
                    var isActiveClaim = identity.SingleOrDefault(p => p.Type == "IsActive");
                    if (isActiveClaim != null)
                    {
                        var removeClaimResult = await _userManager.RemoveClaimAsync(user, isActiveClaim);
                        var addClaimResult = await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(isActiveClaim.Type, customer.Registration.IsActive.ToString()));
                    }
                }
            }

            if (customer.Registration.IsActive)
            {
                await _emailSender.SendEmailAsync(customer.Registration.Email, "Account Modified", $"Your account has been activated, Email: {{customer.Registration.Email}}");
            }
            else
            {
                await _emailSender.SendEmailAsync(customer.Registration.Email, "Account Deactivated", $"Your account has been deactivated.");
            }

            return RedirectToAction("Customers");
        }

        [HttpGet]
        public async Task<IActionResult> ServiceEngineers()
        {
            var model = new ServiceEngineerViewModel();
            var serviceEngineerRole = await _roleManager.FindByNameAsync("Engineer");
            if (serviceEngineerRole != null)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync("Engineer");
                model.ServiceEngineers = usersInRole.ToList();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Registration.IsEdit)
                {
                    var user = await _userManager.FindByEmailAsync(model.Registration.Email);
                    if (user != null)
                    {
                        user.UserName = model.Registration.UserName;
                        var existingClaims = await _userManager.GetClaimsAsync(user);
                        var isActiveClaim = existingClaims.FirstOrDefault(c => c.Type == "IsActive");
                        if (isActiveClaim != null)
                        {
                            await _userManager.RemoveClaimAsync(user, isActiveClaim);
                        }
                        await _userManager.AddClaimAsync(user, new Claim("IsActive", model.Registration.IsActive.ToString()));
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            TempData["SuccessMessage"] = "Service Engineer updated successfully!";
                            return RedirectToAction(nameof(ServiceEngineers));
                        }
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    var user = new IdentityUser { UserName = model.Registration.UserName, Email = model.Registration.Email, EmailConfirmed = true };
                    var result = await _userManager.CreateAsync(user, model.Registration.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Engineer");
                        await _userManager.AddClaimAsync(user, new Claim("IsActive", model.Registration.IsActive.ToString()));
                        TempData["SuccessMessage"] = "Service Engineer created successfully!";
                        return RedirectToAction(nameof(ServiceEngineers));
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            var serviceEngineerRole = await _roleManager.FindByNameAsync("Engineer");
            if (serviceEngineerRole != null)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync("Engineer");
                model.ServiceEngineers = usersInRole.ToList();
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var user = HttpContext.User.GetCurrentUserDetails();
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            return View(new ProfileModel() { UserName = user.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModel profile)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Update UserName
            var currentUser = HttpContext.User.GetCurrentUserDetails();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            var user = await _userManager.FindByEmailAsync(currentUser.Email);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }
            
            user.UserName = profile.UserName;
            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                return View();
            }
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
        }
    }
}
