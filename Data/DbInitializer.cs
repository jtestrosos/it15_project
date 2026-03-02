using manufacturing_system.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace manufacturing_system.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Apply pending migrations automatically
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await context.Database.MigrateAsync();
                }

                // Define roles
                string[] roles = { "Superadmin", "Administrator", "ProductionManager", "ProductionWorker" };

                // Create roles if they don't exist
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Seed a default facility if none exist
                var defaultFacility = await context.Facilities.FirstOrDefaultAsync(f => f.FacilityName == "Main Plant");
                if (defaultFacility == null)
                {
                    defaultFacility = new manufacturing_system.Models.Facility
                    {
                        FacilityName = "Main Plant",
                        Location = "Davao City, PH",
                        SubscriptionStatus = "Active"
                    };
                    context.Facilities.Add(defaultFacility);
                    await context.SaveChangesAsync();
                }

                // Create default Admin account
                var adminEmail = "admin@nodesync.com";
                var adminPassword = "password";

                var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
                if (existingAdmin == null)
                {
                    var adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = "System",
                        LastName = "Administrator",
                        FacilityID = defaultFacility.FacilityID
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Administrator");
                    }
                }
                else
                {
                   // Ensure password is "password" for existing user
                   await userManager.SetLockoutEndDateAsync(existingAdmin, null);
                   await userManager.ResetAccessFailedCountAsync(existingAdmin);
                   
                   var token = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                   await userManager.ResetPasswordAsync(existingAdmin, token, adminPassword);

                   // Ensure facility is assigned
                   if (existingAdmin.FacilityID == null)
                   {
                       existingAdmin.FacilityID = defaultFacility.FacilityID;
                       await userManager.UpdateAsync(existingAdmin);
                   }
                }

                // Create default Manager account
                var managerEmail = "manager@nodesync.com";
                var managerPassword = "password";

                var existingManager = await userManager.FindByEmailAsync(managerEmail);
                if (existingManager == null)
                {
                    var managerUser = new ApplicationUser
                    {
                        UserName = managerEmail,
                        Email = managerEmail,
                        EmailConfirmed = true,
                        FirstName = "Production",
                        LastName = "Manager",
                        FacilityID = defaultFacility.FacilityID
                    };

                    var managerResult = await userManager.CreateAsync(managerUser, managerPassword);
                    if (managerResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(managerUser, "ProductionManager");
                    }
                }
                else
                {
                    // Ensure facility is assigned
                    if (existingManager.FacilityID == null)
                    {
                        existingManager.FacilityID = defaultFacility.FacilityID;
                        await userManager.UpdateAsync(existingManager);
                    }
                }

                // Create default Worker account
                var workerEmail = "worker@nodesync.com";
                var workerPassword = "password";

                var existingWorker = await userManager.FindByEmailAsync(workerEmail);
                if (existingWorker == null)
                {
                    var workerUser = new ApplicationUser
                    {
                        UserName = workerEmail,
                        Email = workerEmail,
                        EmailConfirmed = true,
                        FirstName = "Production",
                        LastName = "Worker",
                        FacilityID = defaultFacility.FacilityID
                    };

                    var workerResult = await userManager.CreateAsync(workerUser, workerPassword);
                    if (workerResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(workerUser, "ProductionWorker");
                    }
                }
                else
                {
                    // Ensure facility is assigned
                    if (existingWorker.FacilityID == null)
                    {
                        existingWorker.FacilityID = defaultFacility.FacilityID;
                        await userManager.UpdateAsync(existingWorker);
                    }
                }

                // Create default Superadmin account
                var superadminEmail = "superadmin@nodesync.com";
                var superadminPassword = "password";

                var existingSuperadmin = await userManager.FindByEmailAsync(superadminEmail);
                if (existingSuperadmin == null)
                {
                    var superadminUser = new ApplicationUser
                    {
                        UserName = superadminEmail,
                        Email = superadminEmail,
                        EmailConfirmed = true,
                        FirstName = "Super",
                        LastName = "Admin",
                        FacilityID = defaultFacility.FacilityID
                    };

                    var superadminResult = await userManager.CreateAsync(superadminUser, superadminPassword);
                    if (superadminResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superadminUser, "Superadmin");
                    }
                }
                else
                {
                   // Ensure password is "password" for existing user
                   await userManager.SetLockoutEndDateAsync(existingSuperadmin, null);
                   await userManager.ResetAccessFailedCountAsync(existingSuperadmin);
                   
                   var token = await userManager.GeneratePasswordResetTokenAsync(existingSuperadmin);
                   await userManager.ResetPasswordAsync(existingSuperadmin, token, superadminPassword);
                }
            }
            catch (Exception ex)
            {
                // Prevent app start failure on database connection issues
                Console.WriteLine("An error occurred during database initialization: " + ex.Message);
            }
        }
    }
}
