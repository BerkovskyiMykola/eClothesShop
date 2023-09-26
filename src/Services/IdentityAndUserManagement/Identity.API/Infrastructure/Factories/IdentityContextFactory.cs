using Identity.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.API.Infrastructure.Factories
{
    public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
               .AddJsonFile("appsettings.json")
               .AddEnvironmentVariables()
               .Build();

            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>()
                .UseSqlServer(config["IdentityDBConnectionString"], sqlServerOptionsAction: x => x.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));

            return new IdentityContext(optionsBuilder.Options);
        }
    }
}
