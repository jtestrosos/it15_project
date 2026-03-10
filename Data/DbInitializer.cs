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

                // Append comprehensive test data if tables are empty
                await SeedTestDataAsync(context, userManager, defaultFacility);
            }
            catch (Exception ex)
            {
                // Prevent app start failure on database connection issues
                Console.WriteLine("An error occurred during database initialization: " + ex.Message);
            }
        }

        private static async Task SeedTestDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, manufacturing_system.Models.Facility facility)
        {
            // Wipe all existing active tracking data and reset ID seeds (Identity) 
            // This ensures a clean slate where IDs start at 1 every time the system starts.
            var tables = new[] { 
                "SystemNotifications", "ActivityLogs", "Costs", "InventoryTransactions", 
                "WorkOrders", "ProductionPlans", "BillOfMaterials", "Products", 
                "Components", "EnvironmentalMonitors" 
            };

            foreach (var table in tables)
            {
                try {
                    await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE {table}");
                } catch {
                    // Fallback for tables with Foreign Key constraints that don't allow TRUNCATE
                    // We delete all and then reset the check-seed manually
                    await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]");
                    await context.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('[{table}]', RESEED, 0)");
                }
            }
            await context.SaveChangesAsync();

            var worker = await userManager.FindByEmailAsync("worker@nodesync.com");
            if (worker == null) return;

            var manager = await userManager.FindByEmailAsync("manager@nodesync.com");

            // 1. Components
            var comp1 = new manufacturing_system.Models.Component { ComponentName = "Steel Chassis", Description = "Heavy duty reinforced steel frame.", UnitCost = 1500.00m, MinStockLevel = 50, SupplierInfo = "IronForge Metals Co.", FacilityID = facility.FacilityID };
            var comp2 = new manufacturing_system.Models.Component { ComponentName = "Lithium Ion Battery Pack", Description = "High capacity 5000mAh battery cell.", UnitCost = 4500.00m, MinStockLevel = 20, SupplierInfo = "VoltEdge Energy", FacilityID = facility.FacilityID };
            var comp3 = new manufacturing_system.Models.Component { ComponentName = "Proximity Sensor Module", Description = "IR based obstacle detection sensor.", UnitCost = 850.00m, MinStockLevel = 100, SupplierInfo = "CyberSense Tech", FacilityID = facility.FacilityID };
            var comp4 = new manufacturing_system.Models.Component { ComponentName = "Rubber Seal", Description = "Waterproof industrial grade rubber seal.", UnitCost = 15.50m, MinStockLevel = 500, SupplierInfo = "FlexiSeal Industries", FacilityID = facility.FacilityID };
            var comp5 = new manufacturing_system.Models.Component { ComponentName = "Brushless Motor", Description = "High torque 12V brushless DC motor.", UnitCost = 2100.00m, MinStockLevel = 30, SupplierInfo = "MotoTech Solutions", FacilityID = facility.FacilityID };
            
            // Computer Parts
            var compCpu = new manufacturing_system.Models.Component { ComponentName = "Intel Core i9-14900K", Description = "High-end processor for workstations.", UnitCost = 35000.00m, MinStockLevel = 50, SupplierInfo = "Intel Authorized Dealer", FacilityID = facility.FacilityID };
            var compGpu = new manufacturing_system.Models.Component { ComponentName = "NVIDIA RTX 4090 24GB", Description = "Leading GPU for gaming and rendering.", UnitCost = 110000.00m, MinStockLevel = 25, SupplierInfo = "GearUp PC Hub", FacilityID = facility.FacilityID };
            var compRam = new manufacturing_system.Models.Component { ComponentName = "64GB DDR5-6000MHz RAM", Description = "High speed dual channel memory kit.", UnitCost = 18000.00m, MinStockLevel = 100, SupplierInfo = "MemoryMax Supplies", FacilityID = facility.FacilityID };
            var compSsd = new manufacturing_system.Models.Component { ComponentName = "4TB NVMe PCIe 4.0 SSD", Description = "Ultra fast storage for large data sets.", UnitCost = 22000.00m, MinStockLevel = 150, SupplierInfo = "StoragePro Inc.", FacilityID = facility.FacilityID };
            var compMobo = new manufacturing_system.Models.Component { ComponentName = "Z790 Premium Motherboard", Description = "High performance board with WiFi 7.", UnitCost = 25000.00m, MinStockLevel = 50, SupplierInfo = "BoardKing Electronics", FacilityID = facility.FacilityID };
            var compPsu = new manufacturing_system.Models.Component { ComponentName = "1200W 80+ Titanium PSU", Description = "Highly efficient power supply unit.", UnitCost = 15000.00m, MinStockLevel = 75, SupplierInfo = "PowerFlow systems", FacilityID = facility.FacilityID };
            var compCase = new manufacturing_system.Models.Component { ComponentName = "Full Tower ATX Case", Description = "Spacious case with optimized airflow.", UnitCost = 12000.00m, MinStockLevel = 100, SupplierInfo = "AeroCool Components", FacilityID = facility.FacilityID };
            var compCooler = new manufacturing_system.Models.Component { ComponentName = "360mm AIO Liquid Cooler", Description = "Triple fan radiator with ARGB pump.", UnitCost = 9000.00m, MinStockLevel = 80, SupplierInfo = "ChillWave Cooling", FacilityID = facility.FacilityID };
            var compServerMobo = new manufacturing_system.Models.Component { ComponentName = "Dual Socket EPYC Motherboard", Description = "Server grade board for high density compute.", UnitCost = 45000.00m, MinStockLevel = 20, SupplierInfo = "NodeSync Hardware", FacilityID = facility.FacilityID };
            var compServerCpu = new manufacturing_system.Models.Component { ComponentName = "AMD EPYC 9654 96-Core", Description = "Massive core count server processor.", UnitCost = 250000.00m, MinStockLevel = 20, SupplierInfo = "AlphaData Solutions", FacilityID = facility.FacilityID };
            var compEccRam = new manufacturing_system.Models.Component { ComponentName = "128GB DDR5 ECC RAM", Description = "Error correcting memory for servers.", UnitCost = 40000.00m, MinStockLevel = 50, SupplierInfo = "DataSecure Memory", FacilityID = facility.FacilityID };

            context.Components.AddRange(comp1, comp2, comp3, comp4, comp5, compCpu, compGpu, compRam, compSsd, compMobo, compPsu, compCase, compCooler, compServerMobo, compServerCpu, compEccRam);
            await context.SaveChangesAsync();

            // 2. Products
            var prod1 = new manufacturing_system.Models.Product { ProductName = "Smart Industrial Drone MK-I", Description = "Automated surveillance drone for warehouse observation.", FacilityID = facility.FacilityID };
            var prod2 = new manufacturing_system.Models.Product { ProductName = "Robotic Arm Controller", Description = "Six-axis capable microcontroller for assembly lines.", FacilityID = facility.FacilityID };
            var prod3 = new manufacturing_system.Models.Product { ProductName = "Titan V9 Gaming PC", Description = "Ultra high-end gaming and rendering workstation.", FacilityID = facility.FacilityID };
            var prod4 = new manufacturing_system.Models.Product { ProductName = "NodeSync Enterprise Blade Server", Description = "High-density compute node for data centers.", FacilityID = facility.FacilityID };
            context.Products.AddRange(prod1, prod2, prod3, prod4);
            await context.SaveChangesAsync();

            // 3. Bill of Materials
            context.BillOfMaterials.AddRange(
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod1.ProductID, ComponentID = comp1.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod1.ProductID, ComponentID = comp2.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod1.ProductID, ComponentID = comp3.ComponentID, QuantityRequired = 4, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod1.ProductID, ComponentID = comp5.ComponentID, QuantityRequired = 4, FacilityID = facility.FacilityID },
                
                // BOM for Titan V9 Gaming PC
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod3.ProductID, ComponentID = compCase.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod3.ProductID, ComponentID = compMobo.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod3.ProductID, ComponentID = compCpu.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod3.ProductID, ComponentID = compGpu.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod3.ProductID, ComponentID = compRam.ComponentID, QuantityRequired = 2, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod3.ProductID, ComponentID = compSsd.ComponentID, QuantityRequired = 2, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod3.ProductID, ComponentID = compPsu.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod3.ProductID, ComponentID = compCooler.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                
                // BOM for NodeSync Enterprise Blade Server
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod4.ProductID, ComponentID = comp1.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod4.ProductID, ComponentID = compServerMobo.ComponentID, QuantityRequired = 1, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod4.ProductID, ComponentID = compServerCpu.ComponentID, QuantityRequired = 2, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod4.ProductID, ComponentID = compEccRam.ComponentID, QuantityRequired = 8, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod4.ProductID, ComponentID = compSsd.ComponentID, QuantityRequired = 4, FacilityID = facility.FacilityID },
                new manufacturing_system.Models.BillOfMaterial { ProductID = prod4.ProductID, ComponentID = compPsu.ComponentID, QuantityRequired = 2, FacilityID = facility.FacilityID }
            );
            await context.SaveChangesAsync();

            // 4. Inventory Transactions (Stock up)
            var now = DateTime.UtcNow;
            context.InventoryTransactions.AddRange(
                new manufacturing_system.Models.InventoryTransaction { ComponentID = comp1.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 250, TransactionDate = now.AddDays(-10), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = comp2.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 50, TransactionDate = now.AddDays(-9), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = comp3.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 400, TransactionDate = now.AddDays(-8), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = comp5.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 200, TransactionDate = now.AddDays(-7), FacilityID = facility.FacilityID },
                
                // Computer parts inbound
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compCpu.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 150, TransactionDate = now.AddDays(-5), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compGpu.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 100, TransactionDate = now.AddDays(-5), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compRam.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 400, TransactionDate = now.AddDays(-4), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compSsd.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 500, TransactionDate = now.AddDays(-4), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compMobo.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 150, TransactionDate = now.AddDays(-3), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compPsu.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 200, TransactionDate = now.AddDays(-2), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compCase.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 150, TransactionDate = now.AddDays(-6), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compCooler.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 180, TransactionDate = now.AddDays(-3), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compServerMobo.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 50, TransactionDate = now.AddDays(-8), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compServerCpu.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 100, TransactionDate = now.AddDays(-8), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.InventoryTransaction { ComponentID = compEccRam.ComponentID, UserID = manager?.Id ?? worker.Id, TransactionType = "Inbound", Quantity = 300, TransactionDate = now.AddDays(-7), FacilityID = facility.FacilityID }
            );
            await context.SaveChangesAsync();

            // 5. Environmental Monitors
            context.EnvironmentalMonitors.AddRange(
                new manufacturing_system.Models.EnvironmentalMonitor { FacilityID = facility.FacilityID, Temperature = 22.5m, Humidity = 45.0m, RecordedDate = now.AddMinutes(-30), AlertStatus = "Safe" },
                new manufacturing_system.Models.EnvironmentalMonitor { FacilityID = facility.FacilityID, Temperature = 23.1m, Humidity = 44.5m, RecordedDate = now.AddMinutes(-15), AlertStatus = "Safe" },
                new manufacturing_system.Models.EnvironmentalMonitor { FacilityID = facility.FacilityID, Temperature = 23.8m, Humidity = 46.0m, RecordedDate = now, AlertStatus = "Safe" }
            );

            // 6. Production Plan
            var plan = new manufacturing_system.Models.ProductionPlan { ProductID = prod1.ProductID, UserID = manager?.Id ?? worker.Id, BatchQuantity = 10, PlannedStartDate = now.AddDays(-2), PlannedEndDate = now.AddDays(2), Status = "Active", FacilityID = facility.FacilityID };
            var plan2 = new manufacturing_system.Models.ProductionPlan { ProductID = prod2.ProductID, UserID = manager?.Id ?? worker.Id, BatchQuantity = 50, PlannedStartDate = now.AddDays(5), PlannedEndDate = now.AddDays(14), Status = "Draft", FacilityID = facility.FacilityID };
            var plan3 = new manufacturing_system.Models.ProductionPlan { ProductID = prod3.ProductID, UserID = manager?.Id ?? worker.Id, BatchQuantity = 25, PlannedStartDate = now.AddDays(-1), PlannedEndDate = now.AddDays(7), Status = "Active", FacilityID = facility.FacilityID };
            var plan4 = new manufacturing_system.Models.ProductionPlan { ProductID = prod4.ProductID, UserID = manager?.Id ?? worker.Id, BatchQuantity = 5, PlannedStartDate = now.AddDays(10), PlannedEndDate = now.AddDays(20), Status = "Draft", FacilityID = facility.FacilityID };
            
            // Extra Production Plans
            var plan5 = new manufacturing_system.Models.ProductionPlan { ProductID = prod2.ProductID, UserID = manager?.Id ?? worker.Id, BatchQuantity = 20, PlannedStartDate = now.AddDays(-5), PlannedEndDate = now.AddDays(3), Status = "Active", FacilityID = facility.FacilityID };
            var plan6 = new manufacturing_system.Models.ProductionPlan { ProductID = prod1.ProductID, UserID = manager?.Id ?? worker.Id, BatchQuantity = 100, PlannedStartDate = now.AddDays(-30), PlannedEndDate = now.AddDays(-20), Status = "Completed", FacilityID = facility.FacilityID };
            var plan7 = new manufacturing_system.Models.ProductionPlan { ProductID = prod3.ProductID, UserID = manager?.Id ?? worker.Id, BatchQuantity = 15, PlannedStartDate = now.AddDays(-25), PlannedEndDate = now.AddDays(-15), Status = "Completed", FacilityID = facility.FacilityID };
            var plan8 = new manufacturing_system.Models.ProductionPlan { ProductID = prod4.ProductID, UserID = manager?.Id ?? worker.Id, BatchQuantity = 2, PlannedStartDate = now.AddDays(-1), PlannedEndDate = now.AddDays(5), Status = "Active", FacilityID = facility.FacilityID };
            
            context.ProductionPlans.AddRange(plan, plan2, plan3, plan4, plan5, plan6, plan7, plan8);
            await context.SaveChangesAsync();

            // 7. Work Orders
            var order1 = new manufacturing_system.Models.WorkOrder { PlanID = plan.PlanID, UserID = worker.Id, OrderDescription = "Assemble Chassis and Mount Motors", Status = "In Progress", BatchQuantity = 10, StartTime = now.AddDays(-1), FacilityID = facility.FacilityID };
            var order2 = new manufacturing_system.Models.WorkOrder { PlanID = plan.PlanID, UserID = worker.Id, OrderDescription = "Install Batteries & Sensors", Status = "Pending", BatchQuantity = 10, FacilityID = facility.FacilityID };
            var order3 = new manufacturing_system.Models.WorkOrder { PlanID = plan3.PlanID, UserID = worker.Id, OrderDescription = "Pre-assemble Motherboard, CPU, RAM and NVMe", Status = "In Progress", BatchQuantity = 25, StartTime = now.AddHours(-10), FacilityID = facility.FacilityID };
            var order4 = new manufacturing_system.Models.WorkOrder { PlanID = plan3.PlanID, UserID = worker.Id, OrderDescription = "Install Motherboard into Case, mount PSU and Cooling", Status = "Pending", BatchQuantity = 25, FacilityID = facility.FacilityID };
            var order5 = new manufacturing_system.Models.WorkOrder { PlanID = plan3.PlanID, UserID = worker.Id, OrderDescription = "Install RTX 4090 GPUs, Cable Management and Boot Test", Status = "Pending", BatchQuantity = 25, FacilityID = facility.FacilityID };
            
            // Extra Work Orders (Historical and Active)
            var order6 = new manufacturing_system.Models.WorkOrder { PlanID = plan6.PlanID, UserID = worker.Id, OrderDescription = "Mass Assembly of Drone MK-I", Status = "Completed", BatchQuantity = 100, StartTime = now.AddDays(-30), EndTime = now.AddDays(-25), FacilityID = facility.FacilityID };
            var order7 = new manufacturing_system.Models.WorkOrder { PlanID = plan6.PlanID, UserID = worker.Id, OrderDescription = "QA Testing and Packaging", Status = "Completed", BatchQuantity = 100, StartTime = now.AddDays(-24), EndTime = now.AddDays(-20), FacilityID = facility.FacilityID };
            
            var order8 = new manufacturing_system.Models.WorkOrder { PlanID = plan7.PlanID, UserID = worker.Id, OrderDescription = "Assemble Premium PC Batch", Status = "Completed", BatchQuantity = 15, StartTime = now.AddDays(-25), EndTime = now.AddDays(-20), FacilityID = facility.FacilityID };
            var order9 = new manufacturing_system.Models.WorkOrder { PlanID = plan7.PlanID, UserID = worker.Id, OrderDescription = "Stress Test and Burn-in", Status = "Completed", BatchQuantity = 15, StartTime = now.AddDays(-19), EndTime = now.AddDays(-15), FacilityID = facility.FacilityID };
            
            var order10 = new manufacturing_system.Models.WorkOrder { PlanID = plan5.PlanID, UserID = worker.Id, OrderDescription = "Calibrate Six-axis Controllers", Status = "In Progress", BatchQuantity = 20, StartTime = now.AddDays(-1), FacilityID = facility.FacilityID };
            var order11 = new manufacturing_system.Models.WorkOrder { PlanID = plan5.PlanID, UserID = worker.Id, OrderDescription = "Flash Firmware and Update", Status = "Pending", BatchQuantity = 20, FacilityID = facility.FacilityID };
            
            var order12 = new manufacturing_system.Models.WorkOrder { PlanID = plan8.PlanID, UserID = worker.Id, OrderDescription = "Rack Mounting and PSU Prep", Status = "In Progress", BatchQuantity = 2, StartTime = now.AddHours(-12), FacilityID = facility.FacilityID };
            var order13 = new manufacturing_system.Models.WorkOrder { PlanID = plan8.PlanID, UserID = worker.Id, OrderDescription = "Slot Server Blades and Run Diagnostics", Status = "Pending", BatchQuantity = 2, FacilityID = facility.FacilityID };

            context.WorkOrders.AddRange(order1, order2, order3, order4, order5, order6, order7, order8, order9, order10, order11, order12, order13);
            await context.SaveChangesAsync();

            // 8. Costs (some initial costs for order1)
            context.Costs.AddRange(
                new manufacturing_system.Models.Cost { OrderID = order1.OrderID, CostType = "Labor", Amount = 1500.00m, CurrencyRate = 59.1699m, RecordedDate = now.AddHours(-5), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.Cost { OrderID = order1.OrderID, ComponentID = comp1.ComponentID, CostType = "Material", Amount = 15000.00m, CurrencyRate = 59.1699m, RecordedDate = now.AddHours(-10), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.Cost { OrderID = order3.OrderID, CostType = "Labor", Amount = 4500.00m, CurrencyRate = 59.1699m, RecordedDate = now.AddHours(-2), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.Cost { OrderID = order3.OrderID, ComponentID = compCpu.ComponentID, CostType = "Material", Amount = (35000m * 25), CurrencyRate = 59.1699m, RecordedDate = now.AddHours(-8), FacilityID = facility.FacilityID },
                new manufacturing_system.Models.Cost { OrderID = order3.OrderID, ComponentID = compMobo.ComponentID, CostType = "Material", Amount = (25000m * 25), CurrencyRate = 59.1699m, RecordedDate = now.AddHours(-8), FacilityID = facility.FacilityID }
            );
            await context.SaveChangesAsync();
        }
    }
}
