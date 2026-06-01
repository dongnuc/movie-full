using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Movie_StructureCode.Domain.Abstractions;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Domain.Repositories;
using Movie_StructureCode.Persistence.Context;
using Movie_StructureCode.Persistence.Repositories;
using Movie_StructureCode.Persistence.Services;

namespace Movie_StructureCode.Persistence.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigurePersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            //  DbContext 
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection")));

            // UnitOfWork 
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            //  Repositories 
            services.AddScoped<ICategoryRepository,   CategoryRepository>();
            services.AddScoped<IMovieRepository,      MovieRepository>();
            services.AddScoped<ITheaterRepository,    TheaterRepository>();
            services.AddScoped<IRoomRepository,       RoomRepository>();
            services.AddScoped<ISeatTypeRepository,   SeatTypeRepository>();
            services.AddScoped<ISeatRepository,       SeatRepository>();
            services.AddScoped<IShowingRepository,    ShowingRepository>();
            services.AddScoped<IShowingSeatRepository, ShowingSeatRepository>();
            services.AddScoped<IBookingRepository,    BookingRepository>();
            services.AddScoped<IBookingDetailRepository, BookingDetailRepository>();
            services.AddScoped<IRepository<RefreshToken>, RefreshTokenRepository>();
            services.AddScoped<ITheaterUserRepository, TheaterUserRepository>();

            // Services
            services.AddScoped<IAccountQueryService, AccountQueryService>();

            return services;
        }
    }
}