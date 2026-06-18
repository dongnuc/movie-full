using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Carter;
using Hangfire;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Movie_StructureCode.API.DependencyInjection.Extensions;
using Movie_StructureCode.API.Hubs;
using Movie_StructureCode.API.Middleware;
using Movie_StructureCode.Application.DependencyInjection.Extensions;
using Movie_StructureCode.Domain.Entities;
using Movie_StructureCode.Infrastructure.DependencyInjection.Extensions;
using Movie_StructureCode.Persistence.DependencyInjection.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// ── PERSISTENCE LAYER ────────────────────────────────────────────────
builder.Services.AddConfigurePersistence(builder.Configuration);

// ── IDENTITY & AUTHENTICATION ────────────────────────────────────────
builder.Services.AddDataProtection();

builder.Services.AddIdentityCore<AppUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<Movie_StructureCode.Persistence.Context.AppDbContext>()
    .AddDefaultTokenProviders();

// ── JWT & AUTHENTICATION ────────────────────────────────────────
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["JWT:ValidIssuer"] ?? "MovieOwner",
        ValidAudience = builder.Configuration["JWT:ValidAudience"] ?? "MovieAL",
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
    };

    // ── SIGNALR: Support JWT via query string for WebSocket connections ──
    // SignalR cannot send Authorization header via WebSocket,
    // so the token is passed as ?access_token=<jwt> query param instead.
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// ── AUTHORIZATION ───────────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SystemAdmin", policy => policy.RequireRole("SystemAdmin"));
    options.AddPolicy("TheaterOwner", policy => policy.RequireRole("TheaterOwner"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

// ── CORS CONFIGURATION ──────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200"
            )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ── INFRASTRUCTURE LAYER ─────────────────────────────────────────────
builder.Services.AddConfigureInfrastructure(builder.Configuration);

// ── LOGGING ──────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration().ReadFrom
    .Configuration(builder.Configuration).CreateLogger();

builder.Logging.ClearProviders().AddSerilog();
builder.Host.UseSerilog();

// ── APPLICATION LAYER ───────────────────────────────────────────────
builder.Services.AddConfigureMediatR();
builder.Services.AddSeatServices();

// ── SIGNALR ──────────────────────────────────────────────────────────
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ── MIDDLEWARE ───────────────────────────────────────────────────────────
builder.Services.AddTransient<ExceptionHandlingMiddleware>();
builder.Services.AddTransient<TokenValidationMiddleware>();

// ── PRESENTATION LAYER ───────────────────────────────────────────────
builder.Services.AddConfigureApi();
builder.Services.AddCarter();
builder.Services.AddEndpointsApiExplorer();

// ── SWAGGER & API DOCUMENTATION ──────────────────────────────────────
builder.Services.AddSwaggerGenNewtonsoftSupport()
                .AddFluentValidationRulesToSwagger()
                .AddSwagger();

// ── API VERSIONING ───────────────────────────────────────────────────
builder.Services.AddApiVersioning(options => options.ReportApiVersions = true)
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                    options.SubstituteApiVersionInUrl = true;
                });

var app = builder.Build();

// Configure the HTTP request pipeline.
// ── CORS MIDDLEWARE ─────────────────────────────────────────────────
app.UseCors("AllowSpecificOrigins");
// ── REQUEST LOGGING & TIMING ────────────────────────────────────────
app.UseMiddleware<RequestLoggingMiddleware>();
// ── EXCEPTION HANDLING ──────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

// ── AUTHENTICATION & AUTHORIZATION ─────────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

// ── HANGFIRE BACKGROUND JOB DASHBOARD ──────────────────────────────
app.UseHangfireDashboard("/hangfire");

// ── CUSTOM TOKEN VALIDATION ────────────────────────────────────────
app.UseMiddleware<TokenValidationMiddleware>();

// ── ENDPOINTS ──────────────────────────────────────────────────────
app.MapCarter();

// ── SIGNALR HUB ROUTES ─────────────────────────────────────────────
app.MapHub<SeatHub>("/hubs/seat");

if (builder.Environment.IsDevelopment() || builder.Environment.IsStaging())
{
}
app.ConfigureSwagger();

try
{
    await app.RunAsync();
    Log.Information("stopped cleanly");
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandle exception occured during bootstrapping");
    await app.StopAsync();
}
finally
{
    Log.CloseAndFlush();
    await app.DisposeAsync();
}

public partial class Programs { }