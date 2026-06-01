using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Movie_StructureCode.Application.Abstractions.Services.Auth;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Application.Abstractions.Services.Email;
using Movie_StructureCode.Contract.Abstractions.Command;
using Movie_StructureCode.Contract.Emails;
using Movie_StructureCode.Infrastructure.Caching;
using Movie_StructureCode.Infrastructure.Services.Auth;
using Movie_StructureCode.Infrastructure.Services.Email;
using Movie_StructureCode.Infrastructure.Caching;
using StackExchange.Redis;

namespace Movie_StructureCode.Infrastructure.DependencyInjection.Extensions
{
    /// <summary>
    /// Infrastructure layer dependency injection configuration
    /// Registers all infrastructure services (Token services, external APIs, etc.)
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add infrastructure services to dependency injection container
        /// </summary>
        public static IServiceCollection AddConfigureInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RedisSettings>(
                configuration.GetSection("Redis"));

            // Register Redis Connection
            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;

                var options = ConfigurationOptions.Parse(settings.Connection);
                options.AbortOnConnectFail = false;

                return ConnectionMultiplexer.Connect(options);
            });

            // Register TokenCache
            services.AddScoped<ITokenCacheService, TokenCacheService>();

            // ?? TOKEN SERVICES ???????????????????????????????????????????????
            // JWT Access Token Provider - handles JWT generation, validation, decode
            services.AddScoped<ITokenProvider, TokenProvider>();

            // Refresh Token Service - handles refresh token operations
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // Token Service Composite - orchestrates token provider + refresh service
            services.AddScoped<ITokenService, TokenServiceComposite>();
            services.AddScoped<ICacheDataService, CacheDataService>();

            //Bind email configuration from appsettings.json
            var emailConfig = new EmailConfiguration();
            configuration.GetSection("EmailConfiguration").Bind(emailConfig);
            services.AddSingleton(emailConfig);

            // Email Service - handles sending emails
            services.AddScoped<IEmailServices, EmailServices>();

            return services;
        }
    }
}
