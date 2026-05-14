using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using RestaurantWithAi.Core.Contracts;
using RestaurantWithAi.Core.Services;
using RestaurantWithAi.Core.Services.Reports;

using RestaurantWithAi.Shared.AI;
using RestaurantWithAi.Shared.Auth;
using RestaurantWithAi.Shared.Waiters;
using RestaurantWithAi.Shared.Dishes;
using RestaurantWithAi.Shared.Reservations;
using RestaurantWithAi.Shared.Restaurants;
using RestaurantWithAi.Shared.Tables;
using RestaurantWithAi.Shared.Orders;
using RestaurantWithAi.Shared.Reviews;
using RestaurantWithAi.Shared.Reports;

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
        services.AddScoped<IReservationsService, ReservationService>();
        services.AddScoped<IRestaurantsService, RestaurantService>();
        services.AddScoped<ITablesService, TableService>();
        services.AddScoped<IOrdersService, OrderService>();
        services.AddScoped<IReviewsService, ReviewService>();
        // Image storage and profile repository
        services.AddSingleton<RestaurantWithAi.Shared.AI.IImageStorageService, S3ImageStorageService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IReviewModerationService, ClaudeReviewModerationService>();
        services.AddScoped<ITextGenerationClient, UnconfiguredTextGenerationClient>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportSectionBuilder, StructuredReportSectionBuilder>();
        services.AddScoped<IReportAnalysisService, ReportAnalysisService>();
        services.AddSingleton<IReportRendererFactory, ReportRendererFactory>();
        services.AddScoped<IWaiterScheduleService, WaiterScheduleService>();
        services.AddScoped<IAdminService, AdminService>();
        return services;
    }
}