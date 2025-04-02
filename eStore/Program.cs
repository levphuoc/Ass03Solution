using BLL.Services.IServices;
using BLL.Services;
using DataAccessLayer.Data;
using DataAccessLayer.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using BLL.Hubs;
using BLL.Services.FirebaseServices;

using Microsoft.AspNetCore.Authentication.Cookies;
using eStore;

using eStore.Components;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using BLL.Mapping;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
// Add HttpClient for API calls
builder.Services.AddHttpClient();

// Add controller support
builder.Services.AddApplicationServices();
// Change from Scoped to Transient to prevent concurrent access issues
// Program.cs hoặc Startup.cs
// Đăng ký DbContext với Scoped thay vì Transient
builder.Services.AddDbContext<EStoreDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"), 
        sqlOptions => 
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60);
            sqlOptions.UseRelationalNulls(false);
        });
    
    // Enable detailed error messages and sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
    
    // Set global query behavior to split queries by default
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.ConfigureWarnings(warnings => 
        warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
});


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
    options.AddPolicy("RequireMember", policy => policy.RequireRole("Admin", "Staff", "Deliverer"));
    options.AddPolicy("RequireUser", policy => policy.RequireRole("Admin", "Staff", "Deliverer", "User"));
});
builder.Services.AddSingleton<IFirebaseDataUploaderService>(provider =>
    new FirebaseDataUploaderService(
        Path.Combine(builder.Environment.WebRootPath, "secrets", "firebase-key.json"),
        "groupassignment03-prn222")
);

FirebaseInitializerService.Initialize(
    Path.Combine(builder.Environment.WebRootPath, "secrets", "firebase-key.json"));


// Business Logic Services
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<ISalesReportService, SalesReportService>();

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
app.MapRazorComponents<eStore.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
