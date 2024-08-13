using ElecShop.Models;
using Microsoft.AspNetCore.Identity;

namespace ElecShop.Services
{
    public class DatabaseInitializer
    {
        public static async Task SeedDataAsync(UserManager<ApplicationUser>? userManager, RoleManager<IdentityRole>? roleManager)
        {
            if (userManager is null || roleManager is null)
            {
                Console.WriteLine("userManager or roleManager is null => exit");
                return;
            }

            var exists = await roleManager.RoleExistsAsync("admin");
            if (!exists)
            {
                Console.WriteLine("Admin role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("admin"));
            }

            exists = await roleManager.RoleExistsAsync("seller");
            if (!exists)
            {
                Console.WriteLine("Seller role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("seller"));
            }

            exists = await roleManager.RoleExistsAsync("client");
            if (!exists)
            {
                Console.WriteLine("Client role is not defined and will be created");
                await roleManager.CreateAsync(new IdentityRole("client"));
            }

            var adminUsers = await userManager.GetUsersInRoleAsync("admin");
            if (adminUsers.Any())
            {
                Console.WriteLine("Admin user already exists => exit");
                return;
            }

            var user = new ApplicationUser()
            {
                FirstName = "Admin",
                LastName = "Admin",
                UserName = "admin@admin.com",
                Email = "admin@admin.com",
                CreatedAt = DateTime.Now
            };

            string initialPassword = "admin123";

            var result = await userManager.CreateAsync(user, initialPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "admin");
                Console.WriteLine("Admin user created successfully! Please update the initial password!");
                Console.WriteLine("Email: " + user.Email);
                Console.WriteLine("Initial password: " + initialPassword);
            }
        }
    }
}
