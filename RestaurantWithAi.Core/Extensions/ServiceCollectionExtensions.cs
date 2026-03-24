using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Dishes;
using RestaurantWithAi.Shared.Restaurants;

namespace RestaurantWithAi.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
            services.AddScoped<IAuthService, CognitoAuthService>();
            services.AddScoped<IDishesService, DishService>();
            services.AddScoped<IRestaurantsService, RestaurantService>();
            return services;
        }
}