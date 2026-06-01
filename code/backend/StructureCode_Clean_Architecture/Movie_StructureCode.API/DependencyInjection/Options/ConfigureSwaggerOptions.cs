using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Movie_StructureCode.API.DependencyInjection.Options
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = AppDomain.CurrentDomain.FriendlyName,
                    Version = description.ApiVersion.ToString(),
                });

            options.MapType<DateOnly>(() => new()
            {
                Format = "date",
                Example = new OpenApiString(DateOnly.MinValue.ToString())
            });

            options.CustomSchemaIds(type => type.ToString().Replace("+", "."));

            // ── JWT BEARER TOKEN CONFIGURATION ──────────────────────────────
            // Thêm JWT Bearer scheme vào Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                    "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                    "Example: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"",
                In = ParameterLocation.Header,
            });

            // Áp dụng JWT Bearer scheme cho tất cả endpoints
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });

            // ── OPERATION FILTER ────────────────────────────────────────────
            // Filter để xử lý [AllowAnonymous] endpoints
            options.OperationFilter<AuthorizeCheckOperationFilter>();
        }
    }

    /// <summary>
    /// Operation filter để remove security requirement từ [AllowAnonymous] endpoints
    /// </summary>
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Kiểm tra xem endpoint có [AllowAnonymous] attribute không
            var hasAllowAnonymous = context.MethodInfo?.DeclaringType?.GetCustomAttributes(
                inherit: true)
                .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>()
                .Any() ?? false;

            var methodHasAllowAnonymous = context.MethodInfo?
                .GetCustomAttributes(inherit: true)
                .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>()
                .Any() ?? false;

            // Nếu endpoint có [AllowAnonymous] → Không cần token
            if (hasAllowAnonymous || methodHasAllowAnonymous)
            {
                // Remove security requirement
                if (operation.Security != null)
                {
                    operation.Security.Clear();
                }
            }
            else
            {
                // Thêm security requirement cho endpoints protected
                if (!operation.Security.Any())
                {
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] { }
                        }
                    });
                }
            }
        }
    }
}
