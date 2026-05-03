using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using production_system.Data;
using production_system.Models;
using System.Security.Claims;

namespace production_system.Services;

/// <summary>
/// Centralized service for logging security, system, and audit events to the database.
/// </summary>
public class AuditLogService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLogService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Log a security event (login, logout, failed login, user creation/deletion, password/role changes, attacks).
    /// </summary>
    public async Task LogSecurityEventAsync(
        string userId,
        string actionType,
        string details,
        string severity = LogSeverity.Info,
        string? entityType = null,
        string? entityId = null,
        int? facilityId = null)
    {
        await WriteLogAsync(userId, actionType, details, LogCategory.Security, severity, entityType, entityId, facilityId);
    }

    /// <summary>
    /// Log a system event (product CRUD, profile updates, system errors).
    /// </summary>
    public async Task LogSystemEventAsync(
        string userId,
        string actionType,
        string details,
        string severity = LogSeverity.Info,
        string? entityType = null,
        string? entityId = null,
        int? facilityId = null)
    {
        await WriteLogAsync(userId, actionType, details, LogCategory.System, severity, entityType, entityId, facilityId);
    }

    /// <summary>
    /// Log an audit/transaction event (purchases, orders, payments, refunds/cancellations).
    /// </summary>
    public async Task LogAuditEventAsync(
        string userId,
        string actionType,
        string details,
        string severity = LogSeverity.Info,
        string? entityType = null,
        string? entityId = null,
        int? facilityId = null)
    {
        await WriteLogAsync(userId, actionType, details, LogCategory.Audit, severity, entityType, entityId, facilityId);
    }

    /// <summary>
    /// Core logging method that writes to the database.
    /// </summary>
    private async Task WriteLogAsync(
        string userId,
        string actionType,
        string details,
        string logCategory,
        string severity,
        string? entityType,
        string? entityId,
        int? facilityId)
    {
        try
        {
            var ipAddress = GetClientIpAddress();

            // If facility ID not provided, look it up from the user
            if (!facilityId.HasValue && !string.IsNullOrEmpty(userId))
            {
                facilityId = await GetUserFacilityIdAsync(userId);
            }

            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var log = new ActivityLog
            {
                UserID = userId,
                ActionType = Truncate(actionType, 50),
                Details = Truncate(details, 500),
                Timestamp = DateTime.UtcNow,
                IPAddress = Truncate(ipAddress, 15),
                FacilityID = facilityId,
                LogCategory = logCategory,
                Severity = severity,
                EntityType = entityType != null ? Truncate(entityType, 50) : null,
                EntityId = entityId != null ? Truncate(entityId, 100) : null
            };

            context.ActivityLogs.Add(log);
            await context.SaveChangesAsync();

            _logger.LogInformation("[{Category}] {Action}: {Details} (User: {UserId}, IP: {IP})",
                logCategory, actionType, details, userId, ipAddress);
        }
        catch (Exception ex)
        {
            // Never let logging failure break the application
            _logger.LogError(ex, "Failed to write audit log: {Action} - {Details}", actionType, details);
        }
    }

    /// <summary>
    /// Gets the current user's ID from HttpContext (when called from SSR pages).
    /// </summary>
    public string? GetCurrentUserId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private string GetClientIpAddress()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return "Unknown";

            // Check for forwarded headers first (behind proxy/load balancer)
            var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    private async Task<int?> GetUserFacilityIdAsync(string userId)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            return user?.FacilityID;
        }
        catch
        {
            return null;
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value[..maxLength];
    }
}
