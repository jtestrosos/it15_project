using production_system.Data;
using production_system.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace production_system.Services
{
    public class NotificationService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Gets unread notifications for a specific user, filtered by their role and facility.
        /// </summary>
        public async Task<List<SystemNotification>> GetNotificationsAsync(
            string userId, int? facilityId, IEnumerable<string> userRoles, int maxCount = 20)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var isSuperadmin = userRoles.Contains("Superadmin");

            var query = context.SystemNotifications
                .Where(n => !n.IsDismissed)
                .Where(n =>
                    // User-specific
                    n.UserID == userId
                    // Or broadcast to facility (no specific user)
                    || (n.UserID == null && (
                        // Global notifications (no facility scope)
                        n.FacilityID == null
                        // Or same facility
                        || n.FacilityID == facilityId
                        // Superadmin sees all broadcasts
                        || isSuperadmin
                    ))
                )
                // Role filter: broadcast needs to match user role (or be global broadcast)
                .Where(n => n.UserID != null || n.TargetRole == null || userRoles.Contains(n.TargetRole));

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(maxCount)
                .ToListAsync();
        }

        /// <summary>
        /// Gets count of unread notifications.
        /// </summary>
        public async Task<int> GetUnreadCountAsync(string userId, int? facilityId, IEnumerable<string> userRoles)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var isSuperadmin = userRoles.Contains("Superadmin");

            return await context.SystemNotifications
                .Where(n => !n.IsDismissed && !n.IsRead)
                .Where(n =>
                    n.UserID == userId
                    || (n.UserID == null && (
                        n.FacilityID == null
                        || n.FacilityID == facilityId
                        || isSuperadmin
                    ))
                )
                .Where(n => n.UserID != null || n.TargetRole == null || userRoles.Contains(n.TargetRole))
                .CountAsync();
        }

        /// <summary>Mark a notification as read.</summary>
        public async Task MarkAsReadAsync(int notificationId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var n = await context.SystemNotifications.FindAsync(notificationId);
            if (n != null)
            {
                n.IsRead = true;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>Mark all notifications as read for a user.</summary>
        public async Task MarkAllAsReadAsync(string userId, int? facilityId, IEnumerable<string> userRoles)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var isSuperadmin = userRoles.Contains("Superadmin");

            var notifications = await context.SystemNotifications
                .Where(n => !n.IsRead && !n.IsDismissed)
                .Where(n =>
                    n.UserID == userId
                    || (n.UserID == null && (
                        n.FacilityID == null
                        || n.FacilityID == facilityId
                        || isSuperadmin
                    ))
                )
                .Where(n => n.UserID != null || n.TargetRole == null || userRoles.Contains(n.TargetRole))
                .ToListAsync();

            foreach (var n in notifications)
                n.IsRead = true;

            await context.SaveChangesAsync();
        }

        /// <summary>Dismiss a notification.</summary>
        public async Task DismissAsync(int notificationId)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var n = await context.SystemNotifications.FindAsync(notificationId);
            if (n != null)
            {
                n.IsDismissed = true;
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Scans the system for important events and generates notifications.
        /// Called periodically or on-demand.
        /// </summary>
        public async Task GenerateSystemAlertsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var cutoff = now.AddHours(-6); // Don't duplicate alerts within 6 hours

            // â”€â”€ 1. Low Stock Alerts â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var components = await context.Components
                .Where(c => !c.IsArchived)
                .ToListAsync();

            foreach (var comp in components)
            {
                // Calculate current stock
                var inbound = await context.InventoryTransactions
                    .Where(t => t.ComponentID == comp.ComponentID && t.TransactionType == "Inbound")
                    .SumAsync(t => (int?)t.Quantity) ?? 0;
                var outbound = await context.InventoryTransactions
                    .Where(t => t.ComponentID == comp.ComponentID && t.TransactionType == "Outbound")
                    .SumAsync(t => (int?)t.Quantity) ?? 0;
                var currentStock = inbound - outbound;

                if (currentStock <= comp.MinStockLevel && comp.MinStockLevel > 0)
                {
                    // Check if we already alerted recently
                    var existing = await context.SystemNotifications.AnyAsync(n =>
                        n.Category == "LowStock"
                        && n.Title.Contains(comp.ComponentName)
                        && n.CreatedAt > cutoff
                        && !n.IsDismissed);

                    if (!existing)
                    {
                        var severity = currentStock <= 0 ? "Critical" : "Warning";
                        context.SystemNotifications.Add(new SystemNotification
                        {
                            FacilityID = comp.FacilityID,
                            TargetRole = "ProductionManager",
                            Title = $"Low stock: {comp.ComponentName}",
                            Message = currentStock <= 0
                                ? $"{comp.ComponentName} is out of stock! Min: {comp.MinStockLevel}, Current: {currentStock}"
                                : $"{comp.ComponentName} is below minimum. Min: {comp.MinStockLevel}, Current: {currentStock}",
                            Category = "LowStock",
                            Severity = severity,
                            LinkUrl = "/inventory/components",
                            CreatedAt = now
                        });
                    }
                }
            }

            // â”€â”€ 2. Environment Alerts â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var recentReadings = await context.EnvironmentalMonitors
                .Where(e => e.RecordedDate > now.AddHours(-2))
                .Where(e => e.AlertStatus == "Critical" || e.AlertStatus == "Warning" || e.AlertStatus == "Cold Warning" || e.AlertStatus == "High Humidity")
                .ToListAsync();

            foreach (var reading in recentReadings)
            {
                var existing = await context.SystemNotifications.AnyAsync(n =>
                    n.Category == "EnvironmentAlert"
                    && n.FacilityID == reading.FacilityID
                    && n.CreatedAt > cutoff
                    && !n.IsDismissed);

                if (!existing)
                {
                    var severity = reading.AlertStatus == "Critical" ? "Critical" : "Warning";
                    context.SystemNotifications.Add(new SystemNotification
                    {
                        FacilityID = reading.FacilityID,
                        TargetRole = null, // Broadcast to everyone in facility (Workers, Managers, Admins)
                        Title = $"Environment {reading.AlertStatus}",
                        Message = $"Temperature: {reading.Temperature}Â°C, Humidity: {reading.Humidity}% â€” Status: {reading.AlertStatus}",
                        Category = "EnvironmentAlert",
                        Severity = severity,
                        LinkUrl = "/monitoring/environment",
                        CreatedAt = now
                    });
                }
            }

            // â”€â”€ 3. Overdue Work Orders â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var overdueOrders = await context.WorkOrders
                .Include(o => o.Plan)
                .Where(o => o.Status == "In Progress" && o.Plan.PlannedEndDate < now)
                .ToListAsync();

            foreach (var order in overdueOrders)
            {
                var existing = await context.SystemNotifications.AnyAsync(n =>
                    n.Category == "WorkOrder"
                    && n.Title.Contains(order.OrderID.ToString())
                    && n.CreatedAt > cutoff
                    && !n.IsDismissed);

                if (!existing)
                {
                    // Notify assigned worker
                    context.SystemNotifications.Add(new SystemNotification
                    {
                        FacilityID = order.FacilityID,
                        UserID = order.UserID,
                        Title = $"Work Order #{order.OrderID} is overdue",
                        Message = $"\"{order.OrderDescription}\" passed its planned end date.",
                        Category = "WorkOrder",
                        Severity = "Warning",
                        LinkUrl = "/tasks/assigned",
                        CreatedAt = now
                    });
                    
            // Also notify the Production Manager (broadcast to role)
                    context.SystemNotifications.Add(new SystemNotification
                    {
                        FacilityID = order.FacilityID,
                        TargetRole = "ProductionManager",
                        Title = $"Worker Overdue: Order #{order.OrderID}",
                        Message = $"\"{order.OrderDescription}\" is overdue.",
                        Category = "WorkOrder",
                        Severity = "Warning",
                        LinkUrl = "/production/workorders",
                        CreatedAt = now
                    });
                }
            }

            // â”€â”€ 3.5. Pending Work Orders (for Workers) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var pendingOrders = await context.WorkOrders
                .Where(o => o.Status == "Pending" && !string.IsNullOrEmpty(o.UserID))
                .ToListAsync();

            foreach (var order in pendingOrders)
            {
                var existing = await context.SystemNotifications.AnyAsync(n =>
                    n.Category == "WorkOrder"
                    && n.UserID == order.UserID
                    && n.Title.Contains("New Task Assigned: #" + order.OrderID.ToString())
                );

                if (!existing)
                {
                    context.SystemNotifications.Add(new SystemNotification
                    {
                        FacilityID = order.FacilityID,
                        UserID = order.UserID,
                        Title = $"New Task Assigned: #{order.OrderID}",
                        Message = $"You've been assigned a new task: \"{order.OrderDescription}\".",
                        Category = "WorkOrder",
                        Severity = "Info",
                        LinkUrl = "/tasks/assigned",
                        CreatedAt = now
                    });
                }
            }

            // â”€â”€ 4. Pending Production Plans (for Managers) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var pendingPlans = await context.ProductionPlans
                .Where(p => p.Status == "Draft" && p.PlannedStartDate <= now.AddDays(2))
                .ToListAsync();

            foreach (var plan in pendingPlans)
            {
                var existing = await context.SystemNotifications.AnyAsync(n =>
                    n.Category == "Production"
                    && n.Title.Contains(plan.PlanID.ToString())
                    && n.CreatedAt > cutoff
                    && !n.IsDismissed);

                if (!existing)
                {
                    context.SystemNotifications.Add(new SystemNotification
                    {
                        FacilityID = plan.FacilityID,
                        TargetRole = "ProductionManager",
                        Title = $"Production Plan #{plan.PlanID} starts soon",
                        Message = $"Planned start: {plan.PlannedStartDate:MMM dd}. Status still \"Draft\" â€” needs approval.",
                        Category = "Production",
                        Severity = "Info",
                        LinkUrl = "/production/plans",
                        CreatedAt = now
                    });
                }
            }

            await context.SaveChangesAsync();

            // â”€â”€ 5. Missing API Keys (for Superadmin) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var facilitiesWithMissingKeys = await context.Facilities
                .Where(f => string.IsNullOrEmpty(f.APIKeyOpenWeather) || string.IsNullOrEmpty(f.APIKeyExchangeRate))
                .ToListAsync();

            if (facilitiesWithMissingKeys.Any())
            {
                var existing = await context.SystemNotifications.AnyAsync(n =>
                    n.Category == "SystemConfig"
                    && n.Title == "Missing API Configuration"
                    && n.CreatedAt > cutoff
                    && !n.IsDismissed);

                if (!existing)
                {
                    context.SystemNotifications.Add(new SystemNotification
                    {
                        FacilityID = null, // Global
                        TargetRole = "Superadmin",
                        Title = "Missing API Configuration",
                        Message = $"{facilitiesWithMissingKeys.Count} facilities are missing OpenWeather or ExchangeRate API keys.",
                        Category = "SystemConfig",
                        Severity = "Warning",
                        LinkUrl = "/admin/system-config",
                        CreatedAt = now
                    });
                }
            }

            // â”€â”€ 6. Unassigned Users (for Administrator) â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
            var unassignedUsersCount = await context.Users
                .Where(u => u.FacilityID == null && !u.IsArchived)
                .CountAsync();

            if (unassignedUsersCount > 0)
            {
                var existing = await context.SystemNotifications.AnyAsync(n =>
                    n.Category == "System"
                    && n.Title == "Pending User Assignments"
                    && n.CreatedAt > cutoff
                    && !n.IsDismissed);

                if (!existing)
                {
                    context.SystemNotifications.Add(new SystemNotification
                    {
                        FacilityID = null, // Global broadcast to Administrator
                        TargetRole = "Administrator",
                        Title = "Pending User Assignments",
                        Message = $"{unassignedUsersCount} users are currently without a facility.",
                        Category = "System",
                        Severity = "Info",
                        LinkUrl = "/admin/users",
                        CreatedAt = now
                    });
                }
            }

            // â”€â”€ 7. Cleanup old dismissed notifications (older than 30 days)
            var oldCutoff = now.AddDays(-30);
            var old = await context.SystemNotifications
                .Where(n => n.IsDismissed && n.CreatedAt < oldCutoff)
                .ToListAsync();
            if (old.Any())
            {
                context.SystemNotifications.RemoveRange(old);
                await context.SaveChangesAsync();
            }
        }
    }
}

