using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASCWeb1.Configuration;
using ASCWeb1.Data;
using ASCWeb1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASCWeb1.Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddConfig(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddOptions();
            services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));

            return services;
        }

        public static IServiceCollection AddMyDependencyGroup(this IServiceCollection services)
        {
            // Đăng ký DbContext
            services.AddScoped<DbContext, ApplicationDbContext>();
            services.AddScoped<ApplicationDbContext>();

            // Đăng ký UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Đăng ký các dịch vụ Identity
            services.AddTransient<ASCWeb1.Services.IEmailSender, AuthMessageSender>();
            services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // QUAN TRỌNG: Chỉ giữ lại 1 dòng Scoped này, XÓA dòng Singleton cũ
            services.AddScoped<IIdentitySeed, IdentitySeed>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Đăng ký Cache và Navigation
            services.AddDistributedMemoryCache();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();

            return services;
        }
    }
}