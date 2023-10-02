using Identity.DAL;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Identity.API;

public class SeedData
{
    public static async Task EnsureIdentitySeedData(IServiceScope scope, IConfiguration configuration, ILogger logger)
    {
        var retryPolicy = CreateRetryPolicy(configuration, logger);
        var context = scope.ServiceProvider.GetRequiredService<IdentityContext>();

        await retryPolicy.ExecuteAsync(async () =>
        {
            await context.Database.MigrateAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var customerRole = await roleManager.FindByNameAsync("Customer");

            if (customerRole == null)
            {
                var result = await roleManager.CreateAsync(new IdentityRole("Customer"));

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                logger.LogDebug("Role customer created");
            }
            else
            {
                logger.LogDebug("Role customer already exists");
            }

            var administratorRole = await roleManager.FindByNameAsync("Administrator");

            if (administratorRole == null)
            {
                var result = await roleManager.CreateAsync(new IdentityRole("Administrator"));

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                logger.LogDebug("Role administrator created");
            }
            else
            {
                logger.LogDebug("Role administrator already exists");
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            var administrator = await userManager.FindByNameAsync("administrator@email.com");

            if (administrator == null)
            {
                administrator = new IdentityUser
                {
                    UserName = "administrator@email.com",
                    Email = "administrator@email.com",
                    EmailConfirmed = true,
                };

                var result = await userManager.CreateAsync(administrator, "Pass123$");

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                result = await userManager.AddToRoleAsync(administrator, "Administrator");

                if (!result.Succeeded)
                {
                    throw new Exception(result.Errors.First().Description);
                }

                logger.LogDebug("Administrator created");
            }
            else
            {
                logger.LogDebug("Administrator already exists");
            }
        });
    }

    private static AsyncPolicy CreateRetryPolicy(IConfiguration configuration, ILogger logger)
    {
        var retryMigrations = false;
        bool.TryParse(configuration["RetryMigrations"], out retryMigrations);

        // Only use a retry policy if configured to do so.
        // When running in an orchestrator/K8s, it will take care of restarting failed services.
        if (retryMigrations)
        {
            return Policy.Handle<Exception>().
                WaitAndRetryForeverAsync(
                    sleepDurationProvider: retry => TimeSpan.FromSeconds(5),
                    onRetry: (exception, retry, timeSpan) => logger.LogWarning(exception, "Error migrating database (retry attempt {retry})", retry));
        }

        return Policy.NoOpAsync();
    }
}