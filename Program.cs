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
        // Relax password requirements for development
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 4;
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

// Register NotificationService for system alerts
builder.Services.AddSingleton<production_system.Services.NotificationService>();

var app = builder.Build();

// Seed roles and admin account
await DbInitializer.SeedRolesAndAdminAsync(app.Services);

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // Ensure we don't mask any detailed errors on the deployed website right now
    // app.UseExceptionHandler("/Error", createScopeForErrors: true);
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

app.Run();


