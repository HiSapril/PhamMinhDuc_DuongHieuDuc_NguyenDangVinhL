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

            // 2. Xử lý Admin, Engineer và Customer
            await CreateOrUpdateUser(userManager, options.Value.AdminEmail, options.Value.AdminName, options.Value.AdminPassword, "Admin");
            await CreateOrUpdateUser(userManager, options.Value.EngineerEmail, options.Value.EngineerName, options.Value.EngineerPassword, "Engineer");
            await CreateOrUpdateUser(userManager, options.Value.CustomerEmail, options.Value.CustomerName, options.Value.CustomerPassword, "User");
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
                    // Thêm claim DisplayName để hiển thị trong UI (không dùng ClaimTypes.Name vì bị ghi đè bởi Identity)
                    await userManager.AddClaimAsync(newUser, new Claim("DisplayName", name));
                }
            }
            else
            {
                // Chỉ cập nhật role nếu chưa có, KHÔNG reset password
                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                // Đảm bảo EmailConfirmed = true
                if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                }
                // Xóa claim DisplayName cũ và thêm mới để đảm bảo đồng bộ
                var existingClaims = await userManager.GetClaimsAsync(user);
                var displayNameClaim = existingClaims.FirstOrDefault(c => c.Type == "DisplayName");
                if (displayNameClaim != null)
                {
                    await userManager.RemoveClaimAsync(user, displayNameClaim);
                }
                await userManager.AddClaimAsync(user, new Claim("DisplayName", name));
                Console.WriteLine($"[!] ĐÃ CẬP NHẬT DISPLAYNAME CLAIM CHO: {email} -> {name}");
            }
        }
    }
}