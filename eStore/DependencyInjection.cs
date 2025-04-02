using BLL.Services.IServices;
using BLL.Services;
using DataAccessLayer.Repository.Interfaces;
using DataAccessLayer.Repository;
using DataAccessLayer.UnitOfWork;

namespace eStore
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();
            services.AddScoped<ISalesReportService, SalesReportService>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();
            return services;
        }
    }
}
