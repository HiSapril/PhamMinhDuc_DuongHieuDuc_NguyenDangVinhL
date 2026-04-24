using ASC.DataAccess.Interfaces;
using ASCWeb1.Services;
using ASCWeb1.Configuration;
using ASCWeb1.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. ĐĂNG KÝ CẤU HÌNH & DATABASE (Từ DependencyInjection) ---
builder.Services.AddConfig(builder.Configuration);

// --- 2. ĐĂNG KÝ IDENTITY (DUY NHẤT TẠI ĐÂY) ---
// Việc đăng ký ở đây giúp tránh lỗi "Scheme already exists"
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; // Để đăng nhập được ngay mà không cần xác thực email
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cấu hình Cookie cho Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});

builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
    options.IterationCount = 100000; // Số vòng lặp chuẩn của ASP.NET Core
});
// --- 3. ĐĂNG KÝ CÁC DỊCH VỤ KHÁC ---
builder.Services.AddMyDependencyGroup();

// --- 4. ĐĂNG KÝ EXTERNAL AUTHENTICATION ---
builder.Services.AddAuthentication(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// --- 4. CẤU HÌNH SESSION ---
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// --- 5. PIPELINE (THỨ TỰ QUAN TRỌNG) ---
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // Phải đặt TRƯỚC Authentication
app.UseAuthentication();
app.UseAuthorization();

// --- 6. ROUTES ---
app.MapControllerRoute(
    name: "areaRoute",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// --- 7. SEED DỮ LIỆU VÀ KHỞI TẠO NAVIGATION CACHE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        Console.WriteLine("---- ĐANG BẮT ĐẦU SEED DỮ LIỆU ----");
        
        // Seed Identity
        var storageSeed = services.GetRequiredService<IIdentitySeed>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var options = services.GetRequiredService<IOptions<ApplicationSettings>>();
        await storageSeed.Seed(userManager, roleManager, options);
        
        // Khởi tạo Navigation Cache
        Console.WriteLine("---- ĐANG KHỞI TẠO NAVIGATION CACHE ----");
        var navigationCache = services.GetRequiredService<INavigationCacheOperations>();
        await navigationCache.CreateNavigationCacheAsync();
        
        Console.WriteLine("---- SEED DỮ LIỆU VÀ CACHE HOÀN TẤT ----");
    }
    catch (Exception ex)
    {
        Console.WriteLine("LỖI KHI CHẠY SEED: " + ex.Message);
    }
}

app.Run();