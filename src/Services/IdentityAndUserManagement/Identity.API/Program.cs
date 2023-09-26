using HealthChecks.UI.Client;
using Identity.API;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.WithProperty("ApplicationContext", typeof(Program).Assembly.GetName().Name!)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(context.Configuration["SeqServerUrl"]!);
});

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

// Fix a problem with chrome. Chrome enabled a new feature "Cookies without SameSite must be secure", 
// the cookies should be expired from https, but in Microservices, the internal communication in aks and docker compose is http.
// To avoid this problem, the policy of cookies should be in Lax mode.
app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Lax });

app.UseIdentityServer();

app.UseAuthorization();
app.MapRazorPages().RequireAuthorization();

app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});

// Apply database migration automatically. Note that this approach is not
// recommended for production scenarios. Consider generating SQL scripts from
// migrations instead.
using (var scope = app.Services.CreateScope())
{
    await SeedData.EnsureIdentitySeedData(scope, app.Configuration, app.Logger);
}

await app.RunAsync();