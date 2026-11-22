using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

            // Ensure the database is created and migrations are applied automatically
            try
            {
                await dbContext.Database.MigrateAsync();
            }
            catch
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            // 1. Seed Roles: Admin, Doctor, Patient
            string[] roles = { "Admin", "Doctor", "Patient" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Default Admin Account (Admin represents Assistant in the ERD)
            const string adminEmail = "admin@medicare.com";
            const string defaultAdminPassword = "Admin@123456";

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

                var createResult = await userManager.CreateAsync(adminUser, defaultAdminPassword);
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
