using Microsoft.Extensions.DependencyInjection;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Shared.Auth;

namespace RestaurantWithAi.Core.Extensions;

public static class ServiceCollectionExtensions
{
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, CognitoAuthService>();
            return services;
        }
}