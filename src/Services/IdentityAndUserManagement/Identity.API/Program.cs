using Identity.API;
using Identity.API.Configurations;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

Log.Logger = CreateSerilogLogger(configuration);
builder.Host.UseSerilog();

try
{
    Log.Information("Starting web host ({ApplicationContext})...", AppName);

    builder.Services.RegisterServices(configuration);

    var app = builder.Build();

    app.RegisterMiddlewares();

    Log.Information("Applying migrations ({ApplicationContext})...", AppName);

    // Apply database migration automatically. Note that this approach is not
    // recommended for production scenarios. Consider generating SQL scripts from
    // migrations instead.
    using (var scope = app.Services.CreateScope())
    {
        await SeedData.EnsureIdentitySeedData(scope, app.Configuration, app.Logger);
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", AppName);
}
finally
{
    Log.CloseAndFlush();
}

Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    return new LoggerConfiguration()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.WithProperty("ApplicationContext", AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(configuration["SeqServerUrl"]!)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

public partial class Program
{
    public static readonly string Namespace = typeof(Program).Assembly.GetName().Name!;
    public static readonly string AppName = Namespace;
}