using Identity.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;

namespace UserManagement.API;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSwagger(configuration)
            .AddControllers(configuration)
            .AddCors(configuration)
            .AddOptions(configuration)
            .AddDbContexts(configuration)
            .AddIdentity(configuration)
            .AddAuthentication(configuration)
            .AddHealthCheck(configuration);

        return services;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "UserManagement HTTP API",
                Version = "v1",
                Description = "The UserManagement Service HTTP API"
            });

            var scheme = new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{configuration["IdentityUrlExternal"]}/connect/authorize"),
                        TokenUrl = new Uri($"{configuration["IdentityUrlExternal"]}/connect/token")
                    }
                },
                Type = SecuritySchemeType.OAuth2
            };

            options.AddSecurityDefinition("OAuth", scheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Id = "OAuth", Type = ReferenceType.SecurityScheme }
                    },
                    new List<string> { }
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityContext>(options =>
        {
            options.UseSqlServer(configuration["IdentityDBConnectionString"], sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
            });
        });

        return services;
    }

    private static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
            })
            .AddEntityFrameworkStores<IdentityContext>()
            .AddDefaultTokenProviders();

        services
            .Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(3);
            });

        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // prevent from mapping "sub" claim to nameidentifier.
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = configuration["IdentityUrlInternal"];
            options.RequireHttpsMetadata = false;
            options.Audience = "user-management";
        });

        return services;
    }

    private static IServiceCollection AddHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        hcBuilder.AddSqlServer(
            configuration["IdentityDBConnectionString"]!,
            name: "IdentityDB-check",
            tags: new string[] { "IdentityDB" });

        return services;
    }

    private static IServiceCollection AddControllers(this IServiceCollection services, IConfiguration configuration)
    {
        // Add framework services.
        services.AddControllers(options =>
        {

        });

        return services;
    }

    private static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder
                .SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });

        return services;
    }

    private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Instance = context.HttpContext.Request.Path,
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Please refer to the errors property for additional details."
                };

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json", "application/problem+xml" }
                };
            };
        });

        return services;
    }
}
