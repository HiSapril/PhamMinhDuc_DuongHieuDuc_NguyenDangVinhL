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
        public static CurrentUser GetCurrentUserDetails(this ClaimsPrincipal principal)
        {
            if (!principal.Claims.Any())
                return null;

            return new CurrentUser
            {
                Name = principal.FindFirst(c => c.Type == ClaimTypes.Name).Value,
                Email = principal.FindFirst(c => c.Type == ClaimTypes.Email).Value,
                Roles = principal.FindAll(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray(),
                IsActive = Boolean.Parse(principal.FindFirst(c => c.Type == "IsActive")?.Value ?? "false")
            };
        }
    }
}
