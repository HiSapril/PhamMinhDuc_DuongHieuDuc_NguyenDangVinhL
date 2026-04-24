using ASCWeb1.Areas.Accounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASCWeb1.Areas.Accounts.Controllers
{
    [Area("Accounts")]
    public class AccountsController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountsController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
                        if (isActiveClaim != null) await _userManager.RemoveClaimAsync(user, isActiveClaim);
                        await _userManager.AddClaimAsync(user, new Claim("IsActive", model.Registration.IsActive.ToString()));
                        var result = await _userManager.UpdateAsync(user);
                        if (result.Succeeded)
                        {
                            TempData["SuccessMessage"] = "Updated!";
                            return RedirectToAction(nameof(ServiceEngineers));
                        }
                        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
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
                        TempData["SuccessMessage"] = "Created!";
                        return RedirectToAction(nameof(ServiceEngineers));
                    }
                    foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            var role = await _roleManager.FindByNameAsync("Engineer");
            if (role != null)
            {
                var users = await _userManager.GetUsersInRoleAsync("Engineer");
                model.ServiceEngineers = users.ToList();
            }
            return View(model);
        }
    }
}
