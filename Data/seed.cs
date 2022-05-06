using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace API.Data
{
    public class seed
    {
        public static async Task SeedUsers(UserManager<AppUser> UserManager, RoleManager<AppRole> RolesManager)
        {

            if (await UserManager.Users.AnyAsync()) return;

            var UserData = await System.IO.File.ReadAllTextAsync("Data/UserSeedDate.json");



            //text.json
            var users = JsonSerializer.Deserialize<List<AppUser>>(UserData);
            if (users == null) return;


            var Roles = new List<AppRole>{
                new AppRole{Name="Member"},
                new AppRole{Name="Admin"},
                new AppRole{Name="Moderator"},
            };
            foreach (var role in Roles)
            {
                await RolesManager.CreateAsync(role);
            }

            foreach (var user in users)
            {
                //like foreach
                user.Photos.First().IsApproved = true;

                user.UserName = user.UserName.ToLower();
                await UserManager.CreateAsync(user, "Pa$$w0rd");
                await UserManager.AddToRoleAsync(user, "Member");

            }
            var admin = new AppUser
            {
                UserName = "admin"
            };
            await UserManager.CreateAsync(admin, "Pa$$w0rd");
            await UserManager.AddToRolesAsync(admin, new[] { "admin", "moderator" });
        }
    }
}