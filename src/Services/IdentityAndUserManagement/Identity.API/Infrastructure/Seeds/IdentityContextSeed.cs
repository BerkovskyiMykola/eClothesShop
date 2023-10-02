using Identity.DAL;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Infrastructure.Seeds;

public class IdentityContextSeed
{
    public async Task SeedAsync(
        IdentityContext context,
        ILogger<IdentityContext> logger,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {

        var customerRole = await roleManager.FindByNameAsync("Customer");

        if (customerRole == null)
        {
            var result = await roleManager.CreateAsync(new IdentityRole("Customer"));

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }

        var administratorRole = await roleManager.FindByNameAsync("Administrator");

        if (administratorRole == null)
        {
            var result = await roleManager.CreateAsync(new IdentityRole("Administrator"));

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }
    }
}
