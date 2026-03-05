using FinancePlanner.API.Extensions;
using FinancePlanner.API.Middleware;
using FinancePlanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Serilog;
using Serilog.Events;

// Configure Serilog early so startup errors are captured
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/financeplanner-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting up");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Services
    builder.Services.AddResponseOptimization();
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddApplicationServices();
    builder.Services.AddCorsPolicy(builder.Environment, builder.Configuration);
    builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwagger();

    // Middleware
    var app = builder.Build();

    app.UseResponseCompression();
    app.UseGlobalExceptionHandler();
    app.UseRequestLogging();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinancePlanner API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Migrate on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<FinancePlannerDbContext>();
        try
        {
            if (db.Database.CanConnect())
            {
                Log.Information("DB connected");
                var pending = db.Database.GetPendingMigrations().ToList();
                if (pending.Count > 0)
                {
                    Log.Information("Applying {Count} pending migration(s)", pending.Count);
                    db.Database.Migrate();
                    Log.Information("Migrations applied");
                }
                else
                {
                    Log.Information("DB up to date");
                }
            }
            else
            {
                Log.Warning("Cannot connect to database");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DB startup error: {Message}", ex.Message);
        }
    }

    Log.Information("Running | Environment: {Environment}", app.Environment.EnvironmentName);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
