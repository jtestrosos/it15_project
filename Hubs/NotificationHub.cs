using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace production_system.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                // Add user to groups based on roles to route RBAC-scoped alerts
                if (user.IsInRole("Superadmin")) await Groups.AddToGroupAsync(Context.ConnectionId, "Superadmin");
                if (user.IsInRole("Administrator")) await Groups.AddToGroupAsync(Context.ConnectionId, "Administrator");
                if (user.IsInRole("ProductionManager")) await Groups.AddToGroupAsync(Context.ConnectionId, "ProductionManager");
                if (user.IsInRole("ProductionWorker")) await Groups.AddToGroupAsync(Context.ConnectionId, "ProductionWorker");
                
                // Personal group for direct user notifications (e.g., Worker-specific tasks)
                var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                }
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
