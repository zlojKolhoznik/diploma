using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Waiters;
using RestaurantWithAi.Shared.Dishes;
using RestaurantWithAi.Shared.Restaurants;
using RestaurantWithAi.Shared.Tables;

namespace RestaurantWithAi.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
            services.AddScoped<IAuthService, CognitoAuthService>();
            services.AddScoped<IWaiterService, WaiterService>();
            services.AddScoped<IDishesService, DishService>();
            services.AddScoped<IRestaurantsService, RestaurantService>();
            services.AddScoped<ITablesService, TableService>();
            return services;
        }
}