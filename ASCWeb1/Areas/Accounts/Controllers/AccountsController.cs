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

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var existingClaims = await _userManager.GetClaimsAsync(user);
                var isActiveClaim = existingClaims.FirstOrDefault(c => c.Type == "IsActive");
                
                bool currentStatus = isActiveClaim != null && bool.Parse(isActiveClaim.Value);
                bool newStatus = !currentStatus;

                if (isActiveClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, isActiveClaim);
                }
                
                await _userManager.AddClaimAsync(user, new Claim("IsActive", newStatus.ToString()));

                return Json(new { success = true, newStatus = newStatus });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
