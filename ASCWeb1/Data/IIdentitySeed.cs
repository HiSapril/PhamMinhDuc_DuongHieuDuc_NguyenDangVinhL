using ASCWeb1.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ASCWeb1.Data
{
    public interface IIdentitySeed
    {
        Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<ApplicationSettings> options);
    }
}
