using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Movie_StructureCode.Application.Abstractions.Services.Cache;
using Movie_StructureCode.Application.Abstractions.Services.SeatService;
using Movie_StructureCode.Application.Abstractions.Services.SeatService.InterfaceSeat;
using Movie_StructureCode.Application.Behaviors;

namespace Movie_StructureCode.Application.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigureMediatR(this IServiceCollection services)
        => services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(AssemblyReference).Assembly))
            .AddTransient(typeof(IPipelineBehavior<,>),typeof(ValidationPipelineBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>),typeof(CachingBehavior<,>))
            //.AddTransient(typeof(IPipelineBehavior<,>),typeof(PerformancePipelineBehavior<,>));
            //.AddTransient(typeof(IPipelineBehavior<,>),typeof(ValidationPipelineBehavior<,>));
            .AddValidatorsFromAssembly(Contract.AssemblyReference.Assembly, includeInternalTypes: true)
            .AddValidatorsFromAssembly(typeof(AssemblyReference).Assembly, includeInternalTypes: true)
            ;



        public static IServiceCollection AddSeatServices(this IServiceCollection services)
        {
            services.AddScoped<ISeatValidator, SeatValidator>();
            services.AddScoped<ISeatLayoutService, SeatLayoutService>();
            services.AddScoped<ISeatLockService, SeatLockService>();

            return services;
        }
    }
}
