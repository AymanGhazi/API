using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            //scope for the service of DataConext or ILogger
            using var scope = host.Services.CreateScope();
            //To Get the Services
            var services = scope.ServiceProvider;
            try
            {
                //Get the required Services

                var context = services.GetRequiredService<DataContext>();

                var UserManager = services.GetRequiredService<UserManager<AppUser>>();

                var RoleManager = services.GetRequiredService<RoleManager<AppRole>>();
                
                await context.Database.MigrateAsync();

                await seed.SeedUsers(UserManager, RoleManager);
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "Something Happened During Seeding the Data or Migrating ");
            }
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
