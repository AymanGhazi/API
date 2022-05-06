using API.Data;
using API.Date;
using API.DTos;
using API.Helpers;
using API.interfaces;
using API.services;
using API.SignalR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace API.extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection ApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            //SignalR tracker of users and connectionIDs
            services.AddSingleton<presenceTracker>();
            //to update last Active filter
            services.AddScoped<LogUserActivity>();
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            
            services.AddScoped<IPhotoService, PhotoService>();

            services.AddScoped<ITokenService, TokenService>();

            services.AddAutoMapper(typeof(AutomapperProfiles).Assembly);

            //JWT json web Token
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}