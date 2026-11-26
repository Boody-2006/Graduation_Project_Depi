using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MediCare.Data.Data;
using MediCare.Data.Entities;

namespace MediCare.Web.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // 1. Apply any pending EF Core Migrations
            await dbContext.Database.MigrateAsync();

            // 2. Seed Roles: Admin, Doctor, Patient
            string[] roles = { "Admin", "Doctor", "Patient" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 3. Seed Default Admin Account (Admin represents Assistant in the ERD)
            const string adminEmail = "admin@medicare.com";
            var adminPassword = configuration["Seed:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger("DbInitializer");
                logger?.LogWarning("Admin account seeding skipped: 'Seed:AdminPassword' configuration is missing or empty. To seed the default administrator account, configure 'Seed:AdminPassword' via User Secrets or Environment Variables.");
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Clinic",
                    LastName = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    // Link Admin to Assistant entity per ERD requirements
                    var assistantExists = await dbContext.Assistants.AnyAsync(a => a.Id == adminUser.Id);
                    if (!assistantExists)
                    {
                        dbContext.Assistants.Add(new Assistant { Id = adminUser.Id });
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            else
            {
                // Ensure Admin password is set to the configured secret
                if (!await userManager.CheckPasswordAsync(adminUser, adminPassword))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
                }

                // Ensure Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }

                // Ensure Assistant record exists
                var assistantExists = await dbContext.Assistants.AnyAsync(a => a.Id == adminUser.Id);
                if (!assistantExists)
                {
                    dbContext.Assistants.Add(new Assistant { Id = adminUser.Id });
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
