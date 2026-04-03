using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Data.Repositories;

namespace RestaurantWithAi.Data.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDishRepository, DishRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            services.AddScoped<IWaiterRepository, WaiterRepository>();
            services.AddScoped<ITableRepository, TableRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            var connectionString = configuration.GetConnectionString("LocalConnection")
                                   ?? configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("No SQL Server connection string found. Configure 'ConnectionStrings:LocalConnection' or 'ConnectionStrings:DefaultConnection'.");

            services.AddDbContext<RestaurantDbContext>(options => options.UseSqlServer(connectionString));
            return services;
        }
}