using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ASC.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        public static CurrentUser? GetCurrentUserDetails(this ClaimsPrincipal principal)
        {
            if (!principal.Claims.Any())
                return null;

            // Ưu tiên lấy DisplayName claim (custom claim), nếu không có thì fallback sang ClaimTypes.Name
            var displayNameClaim = principal.FindFirst(c => c.Type == "DisplayName");
            var nameClaim = principal.FindFirst(c => c.Type == ClaimTypes.Name);
            var emailClaim = principal.FindFirst(c => c.Type == ClaimTypes.Email);
            var pictureClaim = principal.FindFirst(c => c.Type == "picture");

            if (emailClaim == null)
                return null;

            // Ưu tiên DisplayName, nếu không có thì lấy Name, cuối cùng fallback sang phần trước @ của email
            var name = displayNameClaim?.Value ?? nameClaim?.Value;
            if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(emailClaim.Value))
            {
                name = emailClaim.Value.Split('@')[0];
            }

            return new CurrentUser
            {
                Name = name ?? "User",
                Email = emailClaim.Value,
                Roles = principal.FindAll(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray(),
                IsActive = Boolean.Parse(principal.FindFirst(c => c.Type == "IsActive")?.Value ?? "false"),
                ProfilePictureUrl = pictureClaim?.Value ?? string.Empty
            };
        }
    }
}
