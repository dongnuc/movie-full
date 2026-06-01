using Microsoft.Extensions.DependencyInjection;
using Movie_StructureCode.Application.Abstractions.Services.SignalR;
using Movie_StructureCode.API.Services;

namespace Movie_StructureCode.API.DependencyInjection.Extensions
{
    /// <summary>
    /// API layer dependency injection extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Them tat ca API services vao DI container
        /// </summary>
        public static IServiceCollection AddConfigureApi(this IServiceCollection services)
        {
            // HttpContextAccessor - can thiet de truy cap HttpContext trong handlers
            services.AddHttpContextAccessor();

            // -- SIGNALR SEAT HUB -------------------------------------------------
            // SeatHubService lives in API layer (same project as SeatHub) because
            // it needs IHubContext<SeatHub>. It is a pure thin adapter with no
            // business logic - just translates Application calls into SignalR broadcasts.
            services.AddScoped<ISeatHubService, SeatHubService>();

            return services;
        }
    }
}
