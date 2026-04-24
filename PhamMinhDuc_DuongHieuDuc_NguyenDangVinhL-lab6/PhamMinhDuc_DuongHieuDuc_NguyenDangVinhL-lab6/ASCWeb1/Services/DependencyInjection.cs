using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASCWeb1.Configuration;
using ASCWeb1.Data;
using ASCWeb1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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

            // Đăng ký MasterDataOperations
            services.AddScoped<ASC.Business.Interfaces.IMasterDataOperations, ASC.Business.MasterDataOperations>();

            // Đăng ký AutoMapper - scan assembly chứa MappingProfile
            services.AddAutoMapper(typeof(ApplicationDbContext), typeof(ASCWeb1.Areas.Configuration.Models.MappingProfile));

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

        public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = config["Google:Identity:ClientId"] ?? string.Empty;
                    options.ClientSecret = config["Google:Identity:ClientSecret"] ?? string.Empty;
                    
                    // Yêu cầu scope để lấy thông tin profile và avatar
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    
                    // Map các claims từ Google
                    options.ClaimActions.MapJsonKey("picture", "picture");
                    options.ClaimActions.MapJsonKey("urn:google:picture", "picture");
                    
                    // Lưu tokens để có thể truy cập thông tin user
                    options.SaveTokens = true;
                });

            return services;
        }
    }
}