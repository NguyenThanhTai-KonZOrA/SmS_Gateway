
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Web;
using SmsGateway.API.Middleware;
using SmsGateway.Common.ApiClient;
using SmsGateway.Implement.ApplicationDbContext;
using SmsGateway.Implement.Interface;
using SmsGateway.Implement.Repositories;
using SmsGateway.Implement.Repositories.Interface;
using SmsGateway.Implement.Service;
using SmsGateway.Implement.Services;
using SmsGateway.Implement.Services.Interfaces;
using SmsGateway.Implement.SmsFactory.FactoryInterface;
using SmsGateway.Implement.SmsFactory.FactoryService;
using SmsGateway.Implement.SmsFactory.SmsConfigs.VnptSettings;
using SmsGateway.Implement.UnitOfWork;
using SmsGateway.Implement.ViewModels.SmsOtpSettings;

// ================================================================
// 🟢 NLog initialization (before builder)
// ================================================================
var logger = LogManager.Setup()
    .LoadConfigurationFromFile("NLog.config")
    .GetCurrentClassLogger();

var config = LogManager.Configuration;
if (config == null)
{
    Console.WriteLine("❌ NLog configuration is NULL — failed to load.");
}
else
{
    Console.WriteLine($"✅ NLog configuration loaded. Targets: {string.Join(", ", config.AllTargets.Select(t => t.Name))}");
}

logger.Info("🟢 SmsGateway.API initializing...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    // ================================================================
    // 🔧 Configure Logging
    // ================================================================
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();  // ✅ Connects NLog to ASP.NET Core pipeline

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add services to the container.
    #region Get settings from appsettings.json
    logger.Info("🟢 Setup configs initializing...");
    builder.Services.Configure<VnptSmsSettings>(builder.Configuration.GetSection("VnptSmsSettings"));
    builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<VnptSmsSettings>>().Value);
    builder.Services.Configure<SmsRateLimitOptionSettings>(builder.Configuration.GetSection("SmsRateLimit"));
    builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<SmsRateLimitOptionSettings>>().Value);
    builder.Services.Configure<OtpOptionSettings>(builder.Configuration.GetSection("Otp"));
    builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<OtpOptionSettings>>().Value);
    logger.Info("🟢 Setup configs done");
    #endregion

    #region Database setup
    builder.Services.AddDbContext<SmsGatewayDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    #endregion

    // Add authentication with API key scheme
    builder.Services.AddAuthentication("ApiKey")
        .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", null);


    #region Add services to the container.
    logger.Info("🟢  Add services...");
    builder.Services.AddHttpClient<IApiClient, ApiClient>();
    builder.Services.AddSingleton<ISmsFactory, SmsFactory>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<ISmsLoggingService, SmsLoggingService>();
    builder.Services.AddScoped<ISmsRateLimiterService, SmsRateLimiterService>();
    builder.Services.AddScoped<IOtpService, OtpService>();
    builder.Services.AddScoped<ISmsProvider, VnptSmsProvider>();
    builder.Services.AddScoped<ISmsProvider, ViettelSmsProvider>();
    builder.Services.AddScoped<ISmsProvider, MobifoneSmsProvider>();
    builder.Services.AddScoped<ISmsProvider, TwilioSmsProvider>();

    builder.Services.AddTransient<IVnptSmsService, VnptSmsService>();
    builder.Services.AddTransient<IOtpCodeRepository, OtpCodeRepository>();
    builder.Services.AddTransient<ISmsDailyStatisticRepository, SmsDailyStatisticRepository>();
    builder.Services.AddTransient<ISmsSendLogRepository, SmsSendLogRepository>();
    logger.Info("🟢  Add services done");

    // ================================================================
    // 🔧 Configure Logging
    // ================================================================
    logger.Info("🟢  Add NLogs...");
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();  // ✅ Connects NLog to ASP.NET Core pipeline
    logger.Info("🟢 Done Add NLogs!");
    #endregion

    var app = builder.Build();

    // ================================================================
    // 🧱 Initial data
    // ================================================================
    logger.Info("Start Update migration");
    using var scope = app.Services.CreateScope();
    var loggerDev = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<SmsGatewayDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        loggerDev.LogCritical(ex, "❌ Database migration failed at startup");
    }
    logger.Info("End Update migration");

    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    //{
    app.UseSwagger();
    app.UseSwaggerUI();
    //}
    app.UseMiddleware<ApiMiddleware>();
    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    // 🔥 Critical startup failure
    logger.Fatal(ex, "❌ App startup terminated unexpectedly.");
    throw;
}
finally
{
    LogManager.Shutdown(); // ✅ Ensures all logs are flushed before exit
}
