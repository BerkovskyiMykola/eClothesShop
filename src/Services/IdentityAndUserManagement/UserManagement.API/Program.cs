using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using UserManagement.API;

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

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Dictionary.API V1");
    options.EnablePersistAuthorization();
    options.OAuthClientId("user-management-swagger-ui");
    options.OAuthScopes("openid", "profile", "user-management");
    options.OAuthUsePkce();
});

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecks("/liveness", new HealthCheckOptions
{
    Predicate = r => r.Name.Contains("self")
});

app.Run();