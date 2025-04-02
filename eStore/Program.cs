using BLL.Services.IServices;
using BLL.Services;
using DataAccessLayer.Data;
using DataAccessLayer.UnitOfWork;
using eStore.Components;
using Microsoft.EntityFrameworkCore;
using BLL.Hubs;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.Repository;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<EStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Business Logic Services
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<ISalesReportService, SalesReportService>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
//builder.Services.AddRazorComponents()
//    .AddInteractiveServerComponents();

// SignalR
builder.Services.AddSignalR();

// -------------------- App Build --------------------
var app = builder.Build();

app.MapHub<SalesReportHub>("/salesReportHub");
app.MapHub<MemberHub>("/memberHub");

// Test DB Connection
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
