using Duende.IdentityServer;
using Identity.API.Configurations;
using Identity.API.Infrastructure.Configuration;
using Identity.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Identity.API.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbContexts(configuration)
            .AddIdentity(configuration)
            .AddIdentityServer(configuration)
            .AddAuthentication(configuration)
            .AddHealthCheck(configuration)
            .AddRazorPages(configuration);

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

        return services;
    }

    private static IServiceCollection AddIdentityServer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;

                options.IssuerUri = "null";
                options.Authentication.CookieLifetime = TimeSpan.FromHours(2);
            })
            .AddInMemoryIdentityResources(IdentitServerConfig.GetResources())
            .AddInMemoryApiScopes(IdentitServerConfig.GetApiScopes())
            .AddInMemoryApiResources(IdentitServerConfig.GetApis())
            .AddInMemoryClients(IdentitServerConfig.GetClients(configuration))
            .AddAspNetIdentity<IdentityUser>()
            .AddDeveloperSigningCredential();

        return services;
    }

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
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

    private static IServiceCollection AddRazorPages(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddRazorPages();

        return services;
    }
}
