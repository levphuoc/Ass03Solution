using BLL.Services.IServices;
using BLL.Services;
using DataAccessLayer.Data;
using DataAccessLayer.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using BLL.Hubs;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.Repository;
using DataAccessLayer.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using eStore;
using BLL.Mappings;
using eStore.Components;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Thêm CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Thêm controllers cho API
builder.Services.AddControllers();

// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add controller support
builder.Services.AddApplicationServices();
// Change from Scoped to Transient to prevent concurrent access issues
builder.Services.AddDbContext<EStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    // Add this to enable sensitive data logging for development
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    // Add this to help with concurrent requests
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

// Changed from Scoped to Transient to prevent concurrent access issues
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

// Auth providers
builder.Services.AddAuthenticationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

// Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/api/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        // Ensure cookie is always sent to server even in redirects
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnSigningOut = async context =>
        {
            // Clear all cookies on signout
            foreach (var cookie in context.HttpContext.Request.Cookies.Keys)
            {
                context.HttpContext.Response.Cookies.Delete(cookie);
            }
            await Task.CompletedTask;
        };
        // Add debugging for authentication events
        options.Events.OnValidatePrincipal = async context =>
        {
            var roles = context.Principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
            
            Console.WriteLine($"User authenticated. Roles: {string.Join(", ", roles)}");
            await Task.CompletedTask;
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Add policies for each role
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireStaff", policy => policy.RequireRole("Admin", "Staff"));
    options.AddPolicy("RequireMember", policy => policy.RequireRole("Admin", "Staff", "Member"));
    options.AddPolicy("RequireUser", policy => policy.RequireRole("Admin", "Staff", "Member", "User"));
});

// SignalR
builder.Services.AddSignalR();
// -------------------- App Build --------------------
var app = builder.Build();

app.MapHub<SalesReportHub>("/salesReportHub");
app.MapHub<OrderHub>("/orderHub");
app.MapHub<MemberHub>("/memberHub");
app.MapHub<ProductHub>("/productHub");
app.MapHub<CategoryHub>("/categoryHub");


// Test DB Connection
// Test DB Connection and Seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<EStoreDbContext>();
    if (context.Database.CanConnect())
    {
        Console.WriteLine("Connected to SQL Server successfully!");

    }
    else
    {
        Console.WriteLine("Could not connect to SQL Server!");
    }
}

// -------------------- Middleware --------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Sử dụng CORS policy
app.UseCors("AllowAll");

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
