using production_system.Components;
using production_system.Components.Account;
using production_system.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var connectionStringName = builder.Environment.IsDevelopment() ? "DefaultConnection" : "OnlineConnection";
var connectionString = builder.Configuration.GetConnectionString(connectionStringName) ?? builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        // Strong password policy (IAS Compliance)
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 8;
        // Account lockout policy (IAS Compliance)
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Register WeatherService for OpenWeather API
builder.Services.AddHttpClient<production_system.Services.IWeatherService, production_system.Services.WeatherService>();

// Register ExchangeRateService for currency conversion API
builder.Services.AddHttpClient<production_system.Services.IExchangeRateService, production_system.Services.ExchangeRateService>();

// Register FacilityAccessService for facility-scoped data access
builder.Services.AddScoped<production_system.Services.FacilityAccessService>();

// Register FacilityFilterService for Superadmin facility selector state
builder.Services.AddScoped<production_system.Services.FacilityFilterService>();

// Register NotificationService for system alerts
builder.Services.AddSingleton<production_system.Services.NotificationService>();

// Register RecaptchaService for Google reCAPTCHA verification
builder.Services.AddHttpClient<production_system.Services.IRecaptchaService, production_system.Services.RecaptchaService>();

// Register HttpContextAccessor for IP/user resolution in services
builder.Services.AddHttpContextAccessor();

// Register AuditLogService for centralized security, system, and audit logging
builder.Services.AddScoped<production_system.Services.AuditLogService>();

var app = builder.Build();

// Seed roles and admin account
await DbInitializer.SeedRolesAndAdminAsync(app.Services);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
}
else
{
    // Show generic error page in production (IAS Compliance - no stack traces)
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
app.MapHub<production_system.Hubs.NotificationHub>("/notificationHub");

app.Run();


