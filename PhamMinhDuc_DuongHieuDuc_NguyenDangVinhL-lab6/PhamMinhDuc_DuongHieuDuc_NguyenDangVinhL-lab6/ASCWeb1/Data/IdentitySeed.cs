using ASCWeb1.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace ASCWeb1.Data
{
    public class IdentitySeed : IIdentitySeed
    {
        public async Task Seed(UserManager<IdentityUser> userManager,
                               RoleManager<IdentityRole> roleManager,
                               IOptions<ApplicationSettings> options)
        {
            if (options.Value == null || string.IsNullOrEmpty(options.Value.Roles))
            {
                Console.WriteLine("!!! SEED LỖI: Không tìm thấy cấu hình Roles.");
                return;
            }

            // 1. Tạo Roles
            var roles = options.Value.Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var role in roles)
            {
                var roleName = role.Trim();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Xử lý Admin và Engineer
            await CreateOrUpdateUser(userManager, options.Value.AdminEmail, options.Value.AdminName, options.Value.AdminPassword, "Admin");
            await CreateOrUpdateUser(userManager, options.Value.EngineerEmail, options.Value.EngineerName, options.Value.EngineerPassword, "Engineer");
        }

        private async Task CreateOrUpdateUser(UserManager<IdentityUser> userManager,
                                              string email, string name, string password, string role)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                var newUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                // Lệnh này sẽ tự động hash chuẩn mật khẩu từ appsettings.json
                var result = await userManager.CreateAsync(newUser, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newUser, role);
                }
            }
            else
            {
                // Nếu đã có User, ta ép Reset lại Password để đồng bộ Hash mới nhất
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var result = await userManager.ResetPasswordAsync(user, token, password);

                if (result.Succeeded)
                {
                    Console.WriteLine($"[!] DA DONG BO HASH MOI CHO: {email}");
                    Console.WriteLine($"[!] HASH HIEN TAI TRONG DB: {user.PasswordHash}");
                }
            }
        }
    }
}