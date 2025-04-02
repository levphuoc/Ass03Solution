using BLL.Services.IServices;
using BLL.Services;
using DataAccessLayer.Data;
using DataAccessLayer.UnitOfWork;
using eStore.Components;
using Microsoft.EntityFrameworkCore;
using BLL.Hubs;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add controller support
builder.Services.AddControllers();

// Change from Scoped to Transient to prevent concurrent access issues
builder.Services.AddDbContext<EStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    // Add this to enable sensitive data logging for development
    .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
    // Add this to help with concurrent requests
    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

// Changed from Scoped to Transient to prevent concurrent access issues
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

// Authentication
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

builder.Services.AddHttpContextAccessor();

// Business Logic Services - Change from Scoped to Transient for all repository and service registrations
builder.Services.AddTransient<IMemberService, MemberService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<IOrderDetailService, OrderDetailService>();
builder.Services.AddTransient<ISalesReportService, SalesReportService>();
builder.Services.AddTransient<IMemberRepository, MemberRepository>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
builder.Services.AddTransient<ICategoryService, CategoryService>();

// SignalR
builder.Services.AddSignalR();

// -------------------- App Build --------------------
var app = builder.Build();

app.MapHub<SalesReportHub>("/salesReportHub");
app.MapHub<MemberHub>("/memberHub");

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
app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Map controllers
app.MapControllers();

app.Run();
