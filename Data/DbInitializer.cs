using manufacturing_system.Data;
using Microsoft.AspNetCore.Identity;

namespace manufacturing_system.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

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
                    Role = "Administrator"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
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
                    Role = "ProductionManager"
                };

                var managerResult = await userManager.CreateAsync(managerUser, managerPassword);
                if (managerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "ProductionManager");
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
                    Role = "ProductionWorker"
                };

                var workerResult = await userManager.CreateAsync(workerUser, workerPassword);
                if (workerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(workerUser, "ProductionWorker");
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
                    Role = "Superadmin"
                };

                var superadminResult = await userManager.CreateAsync(superadminUser, superadminPassword);
                if (superadminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(superadminUser, "Superadmin");
                }
            }
        }
    }
}
